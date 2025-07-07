using UnityEngine;

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
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    [Header("Acorn")]
    [SerializeField] private AudioClip eatAcornSFX;

    [Header("Pitch")]
    [SerializeField] float lowPitchRange = 0.95f;
    [SerializeField] float highPitchRange = 1.05f;

    private AudioSource audioSource;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerHook playerHook;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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
        playerHealth.OnDamagePlayer += PlayDamageSFX;
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
        playerHealth.OnDamagePlayer -= PlayDamageSFX;
        playerHealth.OnDeathPlayer -= PlayDeathSFX;

        // Acorn
        playerMovement.OnEatAcorn -= PlayEatAcornSFX;
    }
    private void Update()
    {
        if (isWalkSFXRunning && !audioSource.isPlaying)
            PlayWalkSFX();        
        else if (isRopeSwingingSFXRunning && !audioSource.isPlaying)
            PlayRopeSwingingSFX();
    }
    private void PlaySFXOneShot(AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }
    private void PlaySFXSingle(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    private void PlaySFXSingle(AudioClip audioClip, float volume)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }    
    private void StopSFX()
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
            PlaySFXSingle(waterWalkSFX[n]);
        }
        else
        {
            n = Random.Range(0, dustWalkSFX.Length);
            PlaySFXSingle(dustWalkSFX[n]);
        }            
        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //audioSource.pitch = randomPitch;                
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpSFX()
    {
        PlaySFXOneShot(takeOffJumpSFX, 1f);
    }
    private void PlayLandingJumpSFX()
    {
        if (GameManager.Instance.IsWetSurface)
            PlaySFXOneShot(waterLandingJumpSFX, waterLandingJumpVolume);
        else
            PlaySFXOneShot(dustLandingJumpSFX, dustLandingJumpVolume);
        audioSource.volume = 1f;
        //Debug.Log("Played Landing Jumping SFX");
    }
    #endregion
    #region Wall Sliding    
    private void StopWallSlidingSFX()
    {
        audioSource.loop = false;
        StopSFX();        
    }
    private void PlayWallSlidingSFX()
    {
        audioSource.loop = true;
        audioSource.time = 1.5f;
        PlaySFXSingle(wallSlidingSFX);
    }
    #endregion
    #region Wall Jump
    private void PlayWallJumpSFX()
    {
        PlaySFXOneShot(wallJumpSFX, 1f);
    }
    #endregion
    #region Grappling-Hook
    #region Hook
    private void PlayHookThrownSFX()
    {
        PlaySFXOneShot(hookThrownSFX, 1f);
    }
    private void PlayHookAttachedSFX()
    {
        PlaySFXOneShot(hookAttachedSFX, 1f);
    }
    private void PlayHookReleasedSFX()
    {
        PlaySFXOneShot(hookReleaseSFX, 1f);
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
        StopSFX();
        
        audioSource.volume = 1f;                // Reset the Volume to its def value.
    }
    private void PlayRopeSwingingSFX()
    {
        int n;
        
        n = Random.Range(0, ropeSwingSFX.Length);
        PlaySFXSingle(ropeSwingSFX[n],ropeSwingVolume);
        
        //float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        //audioSource.pitch = randomPitch;                
    }
    #endregion
    #endregion
    #region Damage-Death
    private void PlayDamageSFX()
    {
        PlaySFXOneShot(damageSFX, 1f);
    }
    private void PlayDeathSFX()
    {
        PlaySFXOneShot(deathSFX, 1f);
    }
    #endregion
    #region Acorn
    private void PlayEatAcornSFX()
    {
        PlaySFXOneShot(eatAcornSFX, 1f);
    }
    #endregion
}
