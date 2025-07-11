using UnityEngine;
using UnityEngine.Audio;

public class PlayerSFX : MonoBehaviour
{    
    [Header("Walk")]
    [SerializeField] private AudioClip[] waterWalkSFX;
    [SerializeField] private AudioClip[] dustWalkSFX;
    private bool isWalkSFXRunning;

    [Header("Jump")]
    [SerializeField] private AudioClip takeOffJumpSFX;    
    [SerializeField] private AudioClip waterLandingJumpSFX;
    [SerializeField] private AudioClip dustLandingJumpSFX;
    [SerializeField, Range(0f, 1f)] float waterLandingJumpVolume;  // 0.3f
    [SerializeField, Range(0f, 1f)] float dustLandingJumpVolume;  // 0.4f

    [Header("Wall Sliding")]
    [SerializeField] private AudioClip wallSlidingSFX;         

    [Header("Wall Jump")]
    [SerializeField] private AudioClip wallJumpSFX;

    [Header("Grappling Hook")]
    [SerializeField] private AudioClip hookThrownSFX;    
    [SerializeField] private AudioClip hookAttachedSFX;    
    [SerializeField] private AudioClip hookReleaseSFX;    
    [SerializeField] private AudioClip[] ropeSwingSFX;
    [SerializeField, Range(0f, 1f)] float ropeSwingVolume;  // ??f
    private bool isRopeSwingingSFXRunning;

    [Header("Damage")]
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip deathSFX;

    [Header("Acorn")]
    [SerializeField] private AudioClip eatAcornSFX;

    [Header("Pitch")]
    [SerializeField] float lowPitchRange = 0.95f;
    [SerializeField] float highPitchRange = 1.05f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource fxAudioSource;
    [SerializeField] private AudioSource hitAudioSource;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerHook playerHook;

    void Awake()
    {
        //fxAudioSource = GetComponent<AudioSource>();
        //hitAudioSource = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHook = GetComponent<PlayerHook>();
        playerHealth = GetComponent<PlayerHealth>();
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
        playerMovement.OnWallJump += PlayWallJumpSFX;        

        // Grappling-Hook
        playerMovement.OnHookThrown += PlayHookThrownSFX;
        playerHook.OnHookAttached += PlayHookAttachedSFX;
        playerMovement.OnHookRelease += PlayHookReleasedSFX;
        playerMovement.OnStartRopeSwinging += TriggerRopeSwingingSFX;
        playerMovement.OnStopRopeSwinging += StopRopeSwingingSFX;

        // Damage Player
        playerHealth.OnHitFXPlayer += PlayHitSFX;
        playerHealth.OnDeathPlayer += PlayDeathSFX;

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
        playerMovement.OnWallJump -= PlayWallJumpSFX;

        // Grappling-Hook
        playerMovement.OnHookThrown -= PlayHookThrownSFX;
        playerHook.OnHookAttached -= PlayHookAttachedSFX;
        playerMovement.OnHookRelease -= PlayHookReleasedSFX;
        playerMovement.OnStartRopeSwinging -= TriggerRopeSwingingSFX;
        playerMovement.OnStopRopeSwinging -= StopRopeSwingingSFX;

        // Damage Player
        playerHealth.OnHitFXPlayer -= PlayHitSFX;
        playerHealth.OnDeathPlayer -= PlayDeathSFX;

        // Acorn
        playerMovement.OnEatAcorn -= PlayEatAcornSFX;
    }
    private void Update()
    {
        if (isWalkSFXRunning && !fxAudioSource.isPlaying)
            PlayWalkSFX();        
        else if (isRopeSwingingSFXRunning && !fxAudioSource.isPlaying)
            PlayRopeSwingingSFX();
    }
    private void PlaySFXOneShot(AudioSource audioSource, AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }    
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip, float volume)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }
    private void PlaySFXSingle(AudioSource audioSource, AudioClip audioClip, float volume, float startTime)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.time = startTime;
        audioSource.Play();
    }
    private void StopSFX(AudioSource audioSource)
    {
        audioSource.Stop();
    }
    #region Walk
    private void TriggerWalkSFX()
    {
        isWalkSFXRunning = true;
    }
    private void StopWalkSFX()
    {
        isWalkSFXRunning = false;
    }
    private void PlayWalkSFX()
    {
        int n;
        if (GameManager.Instance.IsWetSurface)
        {
            n = Random.Range(0, waterWalkSFX.Length);
            PlaySFXSingle(fxAudioSource, waterWalkSFX[n]);
        }
        else
        {
            n = Random.Range(0, dustWalkSFX.Length);
            PlaySFXSingle(fxAudioSource, dustWalkSFX[n]);
        }            
        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //fxAudioSource.pitch = randomPitch;                
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpSFX()
    {
        PlaySFXOneShot(fxAudioSource, takeOffJumpSFX, 1f);
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
        PlaySFXSingle(fxAudioSource, wallSlidingSFX, 1f, 1.5f);
    }
    #endregion
    #region Wall Jump
    private void PlayWallJumpSFX()
    {
        PlaySFXOneShot(fxAudioSource, wallJumpSFX, 1f);
    }
    #endregion
    #region Grappling-Hook
    #region Hook
    private void PlayHookThrownSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookThrownSFX, 1f);
    }
    private void PlayHookAttachedSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookAttachedSFX, 1f);
    }
    private void PlayHookReleasedSFX()
    {
        PlaySFXOneShot(fxAudioSource, hookReleaseSFX, 1f);
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
        PlaySFXOneShot(hitAudioSource, hitSFX, 1f);
    }
    private void PlayDeathSFX()
    {
        PlaySFXOneShot(fxAudioSource, deathSFX, 1f);
    }
    #endregion
    #region Acorn
    private void PlayEatAcornSFX()
    {
        PlaySFXOneShot(fxAudioSource, eatAcornSFX, 1f);
    }
    #endregion
}
