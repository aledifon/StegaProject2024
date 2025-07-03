using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerMovement playerMovement;

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

    [Header("Acorn")]
    [SerializeField] private AudioClip eatAcornSFX;

    [Header("Pitch")]
    [SerializeField] float lowPitchRange = 0.95f;
    [SerializeField] float highPitchRange = 1.05f;    

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void OnEnable()
    {
        playerMovement.OnStartWalking += TriggerWalkSFX;
        playerMovement.OnStopWalking += StopWalkSFX;

        playerMovement.OnTakeOffJump += PlayTakeOffJumpSFX;
        playerMovement.OnLandingJump += PlayLandingJumpSFX;

        playerMovement.OnStartWallSliding += PlayWallSlidingSFX;
        playerMovement.OnStopWallSliding += StopWallSlidingSFX;

        playerMovement.OnWallJump += PlayWallJumpSFX;

        playerMovement.OnEatAcorn += PlayEatAcornSFX;
    }
    private void OnDisable()
    {
        playerMovement.OnStartWalking -= TriggerWalkSFX;
        playerMovement.OnStopWalking -= StopWalkSFX;

        playerMovement.OnTakeOffJump -= PlayTakeOffJumpSFX;
        playerMovement.OnLandingJump -= PlayLandingJumpSFX;

        playerMovement.OnStartWallSliding -= PlayWallSlidingSFX;
        playerMovement.OnStopWallSliding -= StopWallSlidingSFX;

        playerMovement.OnWallJump -= PlayWallJumpSFX;

        playerMovement.OnEatAcorn -= PlayEatAcornSFX;
    }
    private void Update()
    {
        if (isWalkSFXRunning && !audioSource.isPlaying)
            PlayWalkSFX();        


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
    #region Acorn
    private void PlayEatAcornSFX()
    {
        PlaySFXOneShot(eatAcornSFX, 1f);
    }
    #endregion
}
