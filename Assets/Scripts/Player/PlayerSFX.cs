using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSFX : MonoBehaviour
{    
    [Header("Walk")]
    [SerializeField] private AudioClip[] waterWalkSFX;
    [SerializeField] private AudioClip[] dustWalkSFX;
    [SerializeField, Range(0f, 1f)] float walkVolume;  // 1f
    private bool isWalkSFXRunning;

    [Header("Jump")]
    [SerializeField] private AudioClip takeOffJumpSFX;
    [SerializeField, Range(0f, 1f)] float takeOffJumpVolume;  // 1f
    [SerializeField] private AudioClip waterLandingJumpSFX;
    [SerializeField] private AudioClip dustLandingJumpSFX;
    [SerializeField, Range(0f, 1f)] float waterLandingJumpVolume;  // 0.3f
    [SerializeField, Range(0f, 1f)] float dustLandingJumpVolume;  // 0.4f

    [Header("Wall Sliding")]
    [SerializeField] private AudioClip wallSlidingSFX;
    [SerializeField, Range(0f, 1f)] float wallSlidingVolume;  // 1f

    [Header("Wall Jump")]
    [SerializeField] private AudioClip wallJumpSFX;
    [SerializeField, Range(0f, 1f)] float wallJumpVolume;  // 1f    
    [SerializeField] private AudioClip airSpinSFX;
    [SerializeField, Range(0f, 1f)] float airSpinVolume;  // 1f
    private bool isAirSpinSFXRunning;
    public bool IsAirSpinSFXRunning => isAirSpinSFXRunning;

    [Header("Grappling Hook")]
    [SerializeField] private AudioClip hookThrownSFX;
    [SerializeField, Range(0f, 1f)] float hookThrownVolume;  //1f
    [SerializeField] private AudioClip hookAttachedSFX;
    [SerializeField, Range(0f, 1f)] float hookAttachedVolume;  //1f
    [SerializeField] private AudioClip hookReleaseSFX;
    [SerializeField, Range(0f, 1f)] float hookReleaseVolume;  //1f
    [SerializeField] private AudioClip[] ropeSwingSFX;
    [SerializeField, Range(0f, 1f)] float ropeSwingVolume;  // 0.4f
    private bool isRopeSwingingSFXRunning;

    [Header("Damage")]
    [SerializeField] private AudioClip hitSFX;
    [SerializeField, Range(0f, 1f)] float hitVolume;        //1f
    [SerializeField] private AudioClip deathSFX;
    [SerializeField, Range(0f, 1f)] float deathVolume;      //1f

    [Header("Enemy Jump")]
    [SerializeField] private AudioClip enemyJumpSFX;
    [SerializeField, Range(0f, 1f)] float enemyJumpVolume;  //1f

    [Header("Acorn")]
    [SerializeField] private AudioClip eatAcornSFX;
    [SerializeField, Range(0f, 1f)] float eatAcornVolume;  // 0.4f

    [Header("Pitch")]
    [SerializeField] float lowPitchRange = 0.95f;
    [SerializeField] float highPitchRange = 1.05f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource fxAudioSource;
    [SerializeField] private AudioSource hitAudioSource;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerHook playerHook;

    private AudioClip currentSFX;

    void Awake()
    {
        //fxAudioSource = GetComponent<AudioSource>();
        //hitAudioSource = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHook = GetComponent<PlayerHook>();
        playerHealth = GetComponent<PlayerHealth>();

        GameManager.Instance.SubscribeEventsOfPlayerSFX(this);
    }
    private void OnEnable()
    {
        // Walk
        playerMovement.OnStartWalking += TriggerWalkSFX;
        playerMovement.OnStopWalking += StopWalkSFX;

        // TakeOff Jump
        playerMovement.OnTakeOffJump += PlayTakeOffJumpSFX;
        playerMovement.OnLandingJump += PlayLandingJumpSFX;

        // Wall Sliding
        playerMovement.OnStartWallSliding += PlayWallSlidingSFX;
        playerMovement.OnStopWallSliding += StopWallSlidingSFX;

        // Wall Jump
        //playerMovement.OnWallJump += PlayWallJumpSFX;        
        playerMovement.OnWallJump += PlayAirSpinSFX;
        playerMovement.OnStopAirSpin += StopAirSpinSFX;

        // Grappling-Hook
        playerMovement.OnHookThrown += PlayHookThrownSFX;
        playerHook.OnHookAttached += PlayHookAttachedSFX;
        playerMovement.OnHookRelease += PlayHookReleasedSFX;
        playerMovement.OnStartRopeSwinging += TriggerRopeSwingingSFX;
        playerMovement.OnStopRopeSwinging += StopRopeSwingingSFX;

        // Damage Player
        if (playerHealth != null)
        {
            playerHealth.OnHitFXPlayer += PlayHitSFX;
            playerHealth.OnDeathPlayer += PlayDeathSFX;
        }
        // Acorn
        playerMovement.OnEatAcorn += PlayEatAcornSFX;        
    }
    private void OnDisable()
    {
        // Walk
        playerMovement.OnStartWalking -= TriggerWalkSFX;
        playerMovement.OnStopWalking -= StopWalkSFX;

        // TakeOff Jump
        playerMovement.OnTakeOffJump -= PlayTakeOffJumpSFX;
        playerMovement.OnLandingJump -= PlayLandingJumpSFX;

        // Wall Sliding
        playerMovement.OnStartWallSliding -= PlayWallSlidingSFX;
        playerMovement.OnStopWallSliding -= StopWallSlidingSFX;

        // Wall Jump
        //playerMovement.OnWallJump -= PlayWallJumpSFX;
        playerMovement.OnWallJump -= PlayAirSpinSFX;
        playerMovement.OnStopAirSpin -= StopAirSpinSFX;

        // Grappling-Hook
        playerMovement.OnHookThrown -= PlayHookThrownSFX;
        playerHook.OnHookAttached -= PlayHookAttachedSFX;
        playerMovement.OnHookRelease -= PlayHookReleasedSFX;
        playerMovement.OnStartRopeSwinging -= TriggerRopeSwingingSFX;
        playerMovement.OnStopRopeSwinging -= StopRopeSwingingSFX;

        // Damage Player
        if (playerHealth != null)
        {
            playerHealth.OnHitFXPlayer -= PlayHitSFX;
            playerHealth.OnDeathPlayer -= PlayDeathSFX;
        }
        // Acorn
        playerMovement.OnEatAcorn -= PlayEatAcornSFX;        
    }
    private void Update()
    {
        if (isWalkSFXRunning && !fxAudioSource.isPlaying && !GameManager.Instance.IsPaused)
            PlayWalkSFX();        
        else if (isRopeSwingingSFXRunning && !fxAudioSource.isPlaying && !GameManager.Instance.IsPaused)
            PlayRopeSwingingSFX();
        //else if (isAirSpinSFXRunning && !fxAudioSource.isPlaying && !GameManager.Instance.IsPaused)
        //    PlayAirSpinSFX();
    }
    #region AudioManagement
    private void PlaySFXOneShot(AudioSource audioSource, AudioClip audioClip, float volume)
    {
        currentSFX = audioClip;
        audioSource.PlayOneShot(audioClip, volume);
    }
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        currentSFX = audioClip;
        audioSource.Play();
    }    
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip, float volume)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        currentSFX = audioClip;
        audioSource.Play();
    }
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip, float volume, float startTime)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.time = startTime;
        currentSFX = audioClip;
        audioSource.Play();
    }
    private void StopSFX(AudioSource audioSource)
    {
        audioSource.Stop();        
    }
    public void PauseAllSFX()
    {
        // For the AudioClips played with Play
        if (waterWalkSFX.Contains(currentSFX) || dustWalkSFX.Contains(currentSFX) ||
            ropeSwingSFX.Contains(currentSFX) || currentSFX == wallSlidingSFX)
        {
            fxAudioSource.Pause();
        }
        // For the AudioClips played with PlayOneShot
        else
        {
            fxAudioSource.Stop();
        }
    }
    public void ResumeAllSFX()
    {
        // For the AudioClips played with Play
        if (waterWalkSFX.Contains(currentSFX) || dustWalkSFX.Contains(currentSFX) ||
            ropeSwingSFX.Contains(currentSFX) || currentSFX == wallSlidingSFX)
        {
            fxAudioSource.UnPause();
        }
        // For the AudioClips played with PlayOneShot --> DO NOTHING        
    }
    #endregion
    #region Walk
    private void TriggerWalkSFX()
    {
        isWalkSFXRunning = true;
    }
    private void StopWalkSFX()
    {
        isWalkSFXRunning = false;
        fxAudioSource.volume = 1f;
    }
    private void PlayWalkSFX()
    {
        int n;
        if (GameManager.Instance.IsWetSurface)
        {
            n = Random.Range(0, waterWalkSFX.Length);
            PlaySFXSingle(fxAudioSource, waterWalkSFX[n], walkVolume);
        }
        else
        {
            n = Random.Range(0, dustWalkSFX.Length);
            PlaySFXSingle(fxAudioSource, dustWalkSFX[n], walkVolume);
        }            
        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //fxAudioSource.pitch = randomPitch;                
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpSFX()
    {
        PlaySFXOneShot(fxAudioSource, takeOffJumpSFX, takeOffJumpVolume);
    }
    private void PlayLandingJumpSFX()
    {
        if (GameManager.Instance.IsWetSurface)
            PlaySFXOneShot(fxAudioSource, waterLandingJumpSFX, waterLandingJumpVolume);
        else
            PlaySFXOneShot(fxAudioSource, dustLandingJumpSFX, dustLandingJumpVolume);
        fxAudioSource.volume = 1f;
        //Debug.Log("Played Landing Jumping SFX");
    }
    #endregion
    #region Wall Sliding    
    private void StopWallSlidingSFX()
    {
        fxAudioSource.loop = false;
        StopSFX(fxAudioSource);        
    }
    private void PlayWallSlidingSFX()
    {
        fxAudioSource.loop = true;        
        PlaySFXSingle(fxAudioSource, wallSlidingSFX, wallSlidingVolume, 1.5f);
    }
    #endregion
    #region Wall Jump
    private void PlayWallJumpSFX()
    {
        PlaySFXOneShot(fxAudioSource, wallJumpSFX, wallJumpVolume);
    }

    #region Walk
    private void TriggerAirSpinSFX()
    {
        isAirSpinSFXRunning = true;
    }
    private void StopAirSpinSFX()
    {
        isAirSpinSFXRunning = false;
        //fxAudioSource.volume = 1f;

        fxAudioSource.loop = false;
        StopSFX(fxAudioSource);
    }
    private void PlayAirSpinSFX()
    {
        //int n;        
        //n = Random.Range(0, airSpinSFX.Length);
        //PlaySFXSingle(fxAudioSource, airSpinSFX[n], airSpinVolume);
        
        fxAudioSource.loop = true;
        PlaySFXSingle(fxAudioSource, airSpinSFX, airSpinVolume);
        isAirSpinSFXRunning = true;

        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //fxAudioSource.pitch = randomPitch;                
    }
    #endregion

    #endregion
    #region Grappling-Hook
    #region Hook
    private void PlayHookThrownSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookThrownSFX, hookThrownVolume);
    }
    private void PlayHookAttachedSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookAttachedSFX, hookAttachedVolume);
    }
    private void PlayHookReleasedSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookReleaseSFX, hookReleaseVolume);
    }
    #endregion
    #region Rope Swinging
    private void TriggerRopeSwingingSFX()
    {
        isRopeSwingingSFXRunning = true;
    }
    private void StopRopeSwingingSFX()
    {
        isRopeSwingingSFXRunning = false;
        StopSFX(fxAudioSource);
        
        fxAudioSource.volume = 1f;                // Reset the Volume to its def value.
    }
    private void PlayRopeSwingingSFX()
    {
        int n;
        
        n = Random.Range(0, ropeSwingSFX.Length);
        PlaySFXSingle(fxAudioSource, ropeSwingSFX[n],ropeSwingVolume);
        
        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //fxAudioSource.pitch = randomPitch;                
    }
    #endregion
    #endregion
    #region Damage-Death
    private void PlayHitSFX(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        PlaySFXOneShot(hitAudioSource, hitSFX, hitVolume);
    }
    private void PlayDeathSFX()
    {
        PlaySFXOneShot(fxAudioSource, deathSFX, deathVolume);
    }
    #endregion
    #region Acorn
    private void PlayEatAcornSFX()
    {
        PlaySFXOneShot(fxAudioSource, eatAcornSFX, eatAcornVolume);
    }
    #endregion
    #region Enemy Jump
    public void PlayEnemyJumpSFX()
    {
        PlaySFXOneShot(fxAudioSource, enemyJumpSFX, enemyJumpVolume);
    }
    #endregion
}
