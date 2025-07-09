using Demo_Project;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Audio;

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

    [SerializeField] AudioClip gameOverClip;    

    [Header("Surface Type")]
    [SerializeField] private bool isWetSurface;
    public bool IsWetSurface => isWetSurface;


    [Header("Slow Hit Time")]
    [SerializeField] float elapsedSlowHitTime;
    [SerializeField] float durationSlowHitTime;     // 1f;

    [Header("Slow Hit Time DOTWeen")]
    [SerializeField] float slowDuration;            // 0.3f
    [SerializeField] float returnDuration;          // 0.8f;

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;    
    // Typical Range for Lowpass: 22000 (no filter) - 500 (strong filter)
    [SerializeField] private float lowPassMaxFreq = 22000f;
    [SerializeField] private float lowPassMinFreq = 450f;
    [SerializeField] private float volumeMaxValue = 0f;
    [SerializeField] private float volumeMinValue = -15f;
    [SerializeField] private float filterDuration = 0.5f;       //0.5f
    [SerializeField] private float volumeDropDuration = 0.5f;   //0.5f
    private string sfxLowPassParam = "SFXLowpassFreq";
    private string sfxVolumeParam = "SFXVolume";
    private string musicLowPassParam = "MusicLowpassFreq";
    private string musicVolumeParam = "MusicVolume";
    #region Events & Delegates
    public event Action<Vector2, float> OnHitPhysicsPlayer;
    #endregion

    // GO Refs.
    AudioSource generalAudioSource;
    PlayerHealth playerHealth;

    #region Unity API
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        generalAudioSource = GetComponent<AudioSource>();

        // Set the filterDuration
        //filterDuration = returnDuration;


    }    
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHitFXPlayer -= SlowMotionOnHit;
            playerHealth.OnHitFXPlayer -= ApplyDeafeningSFX;
        }
    }
    #endregion
    #region Events Subscriptions
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnHitFXPlayer += SlowMotionOnHit;
        playerHealth.OnHitFXPlayer += ApplyDeafeningSFX;
    }
    #endregion
    #region GameOver
    public void PlayGameOverSFx()
    {
        if(generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.PlayOneShot(gameOverClip);
    }
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
        DOVirtual.DelayedCall(slowDuration, () =>
        {            
            // 4. Cuando termina la espera, comienza el tween para interpolar Time.timeScale de 0 a 1
            DOTween.To(() => Time.timeScale, 
                    x => 
                        {
                        Time.timeScale = x;
                        Time.fixedDeltaTime = 0.02f * x;
                        }, 
                    1f, returnDuration)
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
        ApplyDeafSFX()
            .AppendInterval(slowDuration)
            .AppendCallback(() => RemoveDeafSFX());
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
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMinFreq, filterDuration, Ease.OutQuad));
        // Low Pass Filter applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMinFreq, filterDuration, Ease.OutQuad));

        // Volume attenuation applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMinValue, volumeDropDuration, Ease.OutQuad));
        // Volume attenuation applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMinValue, volumeDropDuration, Ease.OutQuad));

        return sequence;    
    }
    private Sequence RemoveDeafSFX()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Low Pass Filter disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMaxFreq, returnDuration, Ease.OutQuad));
        // Low Pass Filter disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMaxFreq, returnDuration, Ease.OutQuad));

        // Volume attenuation disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMaxValue, returnDuration, Ease.OutQuad));
        // Volume attenuation disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMaxValue, returnDuration, Ease.OutQuad));

        return sequence;
    }
    #endregion
}
