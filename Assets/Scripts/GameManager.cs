using Demo_Project;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Singleton instance of GameManager
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        { 
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();

                if (instance == null)
                {
                    Debug.LogWarning("No GameManager found in scene. Creating a new one.");
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance; 
        }
    }

    [Header("Audio Clips")]
    [SerializeField] AudioClip gameOverClip;
    [SerializeField, Range(0f, 1f)] float gameOverVolume;  
    [SerializeField] AudioClip endOfLevelClip;
    [SerializeField, Range(0f, 1f)] float endOfLevelVolume;  
    [SerializeField] AudioClip LevelMusicClip;
    [SerializeField, Range(0f, 1f)] float levelMusicVolume;  // 0.25f

    [Header("Surface Type")]
    [SerializeField] private bool isWetSurface;
    public bool IsWetSurface => isWetSurface;

    [Header("Slow Hit Time")]
    [SerializeField] float elapsedSlowHitTime;
    [SerializeField] float durationSlowHitTime;     // 0.5f;

    [Header("Slow Hit Time DOTWeen")]
    [SerializeField] float slowMotAndDeafDelayDuration;     // 0.3f   slowduration
    [SerializeField] float DeafBwdDuration;                 // 0.8f;  returnduration

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;    
    // Typical Range for Lowpass: 22000 (no filter) - 500 (strong filter)
    // Envolving Cave feeling --> CutoffFreq = 8 KHz; Resonance = 1.5-2.5
    [SerializeField] private float lowPassMaxFreq = 8000f;
    [SerializeField] private float lowPassMinFreq = 450f;
    [SerializeField] private float volumeMaxValue = 0f;
    [SerializeField] private float volumeMinValue = -15f;
    [SerializeField] private float filterDeafFwdDuration = 0.8f;       //0.8f  filterduration
    [SerializeField] private float volumeDropDeafFwdDuration = 0.5f;   //0.5f  volumedropduration
    private string sfxLowPassParam = "SFXLowpassFreq";
    private string sfxVolumeParam = "SFXVolume";
    private string musicLowPassParam = "MusicLowpassFreq";
    private string musicVolumeParam = "MusicVolume";    
    #region Events & Delegates
    public event Action<Vector2, float> OnHitPhysicsPlayer;
    public event Action OnPauseEnabled;
    public event Action OnPauseDisabled;
    #endregion

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Slow Motion Test")]
    [SerializeField] private bool slowMotionEnabled;

    // Checkpoint    
    [Header("Starting Pos")]
    [SerializeField] private Transform initPos;
    [SerializeField] private CamBoundariesTriggerArea initCamBoundTriggerArea;
    private CamTriggerAreaData lastCheckpointData = new CamTriggerAreaData();    
    public CamTriggerAreaData LastCheckpointData => lastCheckpointData;    

    // GO Refs.
    AudioSource generalAudioSource;
    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    PlayerSFX playerSFX;

    // Input Player Management    
    private InputActionAsset inputActions;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    #region Unity API
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }            
        
        instance = this;
        DontDestroyOnLoad(gameObject);

        generalAudioSource = GetComponent<AudioSource>();
        PlayLevelMusic();

        EnableGameplayInput();

        if(slowMotionEnabled)
            EnableSlowMotion();

        // Set the filterDuration
        //filterDuration = returnDuration;

        // Set the Initial CheckPoint Data
        SetInitCheckPointData();
    }
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHitFXPlayer -= SlowMotionOnHit;
            playerHealth.OnHitFXPlayer -= ApplyDeafeningSFX;
        }

        if (playerMovement != null)
        {
            OnHitPhysicsPlayer -= playerMovement.ReceiveDamage;            
        }

        if (playerSFX != null)
        {
            OnPauseEnabled -= playerSFX.PauseAllSFX;            
            OnPauseDisabled -= playerSFX.ResumeAllSFX;            
        }        

        instance = null;
    }
    #endregion
    #region Events Subscriptions
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnHitFXPlayer += SlowMotionOnHit;
        playerHealth.OnHitFXPlayer += ApplyDeafeningSFX;
    }
    public void SubscribeEventsOfPlayerSFX(PlayerSFX pSFX)
    {
        playerSFX = pSFX;
        OnPauseEnabled += playerSFX.PauseAllSFX;
        OnPauseDisabled += playerSFX.ResumeAllSFX;
    }
    public void SubscribeEventsOfPlayerMovement(PlayerMovement pM)
    {
        playerMovement = pM;
        OnHitPhysicsPlayer += playerMovement.ReceiveDamage;
    }
    #endregion
    #region Input Action Maps
    public void GetInputActionMaps(InputActionAsset inputActionsAsset)
    {
        inputActions = inputActionsAsset;
    }    
    public void EnableGameplayInput()
    {
        // Enable the Gameplay Action Map & Disable the Pause Action Map
        inputActions.FindActionMap("Gameplay").Enable();
        inputActions.FindActionMap("UI").Disable();
    }
    public void EnablePauseInput()
    {
        // Enable the Pause Action Map & Disable the Gamplay Action Map
        inputActions.FindActionMap("Gameplay").Disable();
        inputActions.FindActionMap("UI").Enable();
    }
    public void DisableAllInputs()
    {
        inputActions.FindActionMap("Gameplay").Disable();
        inputActions.FindActionMap("UI").Disable();
    }
    #endregion
    #region Input Player
    public void PauseResumeGameInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)                    
            TooglePause();
    }
    public void QuitGame(InputAction.CallbackContext context)
    {
        //if (isWebGL)
        //    SceneManager.LoadScene(Scenes.Menu.ToString());
        //else
            QuitGame();
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
    #else
        Application.Quit();            
    #endif
    }
    #endregion
    #region Audio Clips
    #region Level Music
    public void PlayLevelMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.loop = true;
        generalAudioSource.clip = LevelMusicClip;
        generalAudioSource.volume = levelMusicVolume;
        generalAudioSource.Play();
    }
    #endregion
    #region Game Over
    public void PlayGameOverSFx()
    {
        if (generalAudioSource.isPlaying)
        {
            generalAudioSource.Stop();
            generalAudioSource.loop = false;
        }            

        generalAudioSource.PlayOneShot(gameOverClip,gameOverVolume);
    }
    #endregion
    #region EndOfLevel
    public void PlayEndOfLevelSFx()
    {
        if (generalAudioSource.isPlaying)
        {
            generalAudioSource.Stop();
            generalAudioSource.loop = false;
        }            

        generalAudioSource.PlayOneShot(endOfLevelClip,endOfLevelVolume);
    }
    #endregion
    #endregion
    #region Slow Motion
    public void EnableSlowMotion()
    {
        Time.timeScale = 0.2f; // Velocidad al 20%
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Ajustar física
    }
    private void SlowMotionOnHit(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        // 1. Mata cualquier tween previo con el ID "SlowTime"
        DOTween.Kill("SlowTime");

        // 2. Pone el tiempo a 0 instantáneamente
        Time.timeScale = 0.1f; //0.2f;

        // 3. Usa DOVirtual.DelayedCall para esperar slowDuration segundos en tiempo real
        DOVirtual.DelayedCall(slowMotAndDeafDelayDuration, () =>
        {            
            // 4. Cuando termina la espera, comienza el tween para interpolar Time.timeScale de 0 a 1
            DOTween.To(() => Time.timeScale, 
                    x => 
                        {
                        Time.timeScale = x;
                        Time.fixedDeltaTime = 0.02f * x;
                        }, 
                    1f, DeafBwdDuration)
                //.SetEase(Ease.OutCubic)
                .SetEase(Ease.InQuad)
                // 5. Le da un ID para poder controlar o cancelar este tween luego
                .SetId("SlowTime")
                // 6. Hace que el tween corra usando tiempo real, ignorando Time.timeScale
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    // Trigger the OnHitPhysicsPlayer Event  
                    OnHitPhysicsPlayer?.Invoke(thrustEnemyDir, thrustEnemyForce);                    
                });
        })
        // 7. Hace que la llamada retrasada también use tiempo real para contar correctamente
        .SetUpdate(true);
    }
    private void SlowMotionOnHit_()
    {
        StartCoroutine(nameof(SlowMotionOnHitCoroutine));
    }    
    IEnumerator SlowMotionOnHitCoroutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.3f);

        elapsedSlowHitTime = 0f;

        while (elapsedSlowHitTime < durationSlowHitTime)
        {
            float t = elapsedSlowHitTime / durationSlowHitTime;
            Time.timeScale = Mathf.Lerp(0f,1f,t);
                        
            yield return null;
            elapsedSlowHitTime += Time.unscaledDeltaTime;
        }        

        Time.timeScale = 1f;
    }
    #endregion
    #region DeafeningAudioEffect    
    private void ApplyDeafeningSFX(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        float sfxVolumeValue1;
        float sfxVolumeValue2;
        float musicVolumeValue1;        
        float musicVolumeValue2;

        audioMixer.GetFloat(sfxVolumeParam, out sfxVolumeValue1);
        audioMixer.GetFloat(musicVolumeParam, out musicVolumeValue1);

        Debug.Log("Music Volume Before = " + musicVolumeValue1);
        Debug.Log("SFX Volume Before = " + sfxVolumeValue1);

        ApplyDeafSFX()
            .AppendInterval(slowMotAndDeafDelayDuration)
            .AppendCallback(() => RemoveDeafSFX());

        audioMixer.GetFloat(sfxVolumeParam, out sfxVolumeValue2);
        audioMixer.GetFloat(sfxVolumeParam, out musicVolumeValue2);

        Debug.Log("Music Volume After = " + musicVolumeValue2);
        Debug.Log("SFX Volume After = " + sfxVolumeValue2);
    }
    private Tween AudioMixerParamInterpolation(AudioMixer audiomixer, string param, float targetValue, float duration, Ease easeType)
    {
        // Attenuation for SFX Group
        return DOTween.To(() =>
        {
            float currentValue;
            audioMixer.GetFloat(param, out currentValue);
            return currentValue;
        },
                x => audioMixer.SetFloat(param, x),
                targetValue,
                duration
        ).SetEase(easeType);
    }
    private Sequence ApplyDeafSFX()
    {    
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Low Pass Filter applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMinFreq, filterDeafFwdDuration, Ease.OutQuad));
        // Low Pass Filter applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMinFreq, filterDeafFwdDuration, Ease.OutQuad));

        // Volume attenuation applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMinValue, volumeDropDeafFwdDuration, Ease.OutQuad));
        // Volume attenuation applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMinValue, volumeDropDeafFwdDuration, Ease.OutQuad));

        return sequence;    
    }
    private Sequence RemoveDeafSFX()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Low Pass Filter disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMaxFreq, DeafBwdDuration, Ease.OutQuad));
        // Low Pass Filter disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMaxFreq, DeafBwdDuration, Ease.OutQuad));

        // Volume attenuation disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMaxValue, DeafBwdDuration, Ease.OutQuad));
        // Volume attenuation disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMaxValue, DeafBwdDuration, Ease.OutQuad));

        return sequence;
    }
    #endregion
    #region Pause-Resume    
    private void TooglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            EnablePauseInput();
            PauseGame();
            OnPauseEnabled?.Invoke();
        }
        else
        {
            EnableGameplayInput();
            ResumeGame();
            OnPauseDisabled?.Invoke();
        }
    }    
    private void PauseGame()
    {
        Time.timeScale = 0f;                // Stops the game (stop the physics and pending updates which are time dependent)

        // Pause the Background Music
        generalAudioSource.Pause();

        // UI Panels Update
        pausePanel.SetActive(true);

        //if (sceneSelected != Scenes.Menu)
        //    ShowMouseCursor(true);
        // Update the Panel Selected State
        //panelSelected = PanelSelected.Pause;
    }
    private void ResumeGame()
    {
        Time.timeScale = 1f;                // Resumes the game

        // Resume the Background Music
        generalAudioSource.UnPause();

        // UI Panels Update
        pausePanel.SetActive(false);

        //if (sceneSelected != Scenes.Menu)
        //    ShowMouseCursor(false);
        //panelSelected = PanelSelected.Game;     // As the Pause can be launch from any Panel this could be wrong (NEEDED TO UPDATE!)
    }
    #endregion
    #region CheckPoint
    private void SetInitCheckPointData()
    {
        lastCheckpointData.camTriggerAreaId = CamTriggerAreaEnum.CamTriggerArea.Init;
        if (initPos == null || initCamBoundTriggerArea == null)
            Debug.LogError("No any Init Respawn Pos and/or Cam Bound Trigger Area were assigned!");
        else
        {
            lastCheckpointData.respawnPos = initPos;
            lastCheckpointData.respawnCamBoundTriggerArea = initCamBoundTriggerArea;
        }
    }
    public void SetLastCheckPointData(CamTriggerAreaData camTriggerAreaData)
    {
        lastCheckpointData = camTriggerAreaData;        

        // Update the Player's position
    }    
    #endregion
}
