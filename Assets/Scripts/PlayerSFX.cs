using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerMovement playerMovement;

    [Header("Walk")]
    [SerializeField] private AudioClip[] waterWalkSFX;
    //[SerializeField] private AudioClip dustWalkSFX;    
    private bool isWalkSFXRunning;

    [Header("Jump")]
    [SerializeField] private AudioClip waterTakeOffJumpSFX;
    //[SerializeField] private AudioClip dustTakeOffJumpSFX;            
    [SerializeField] private AudioClip waterLandingJumpSFX;
    //[SerializeField] private AudioClip dustLandingJumpSFX;            

    [Header("Wall Sliding")]
    [SerializeField] private AudioClip wallSlidingSFX;     
    private bool isWallSlidingSFXRunning;

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
    }
    private void OnEnable()
    {
        playerMovement.OnStartWalking += TriggerWalkSFX;
        playerMovement.OnStopWalking += StopWalkSFX;

        playerMovement.OnTakeOffJump += PlayTakeOffJumpSFX;
        playerMovement.OnLandingJump += PlayLandingJumpSFX;

        playerMovement.OnStartWallSliding += TriggerWallSlidingSFX;
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

        playerMovement.OnStartWallSliding -= TriggerWallSlidingSFX;
        playerMovement.OnStopWallSliding -= StopWallSlidingSFX;

        playerMovement.OnWallJump -= PlayWallJumpSFX;

        playerMovement.OnEatAcorn -= PlayEatAcornSFX;
    }
    private void Update()
    {
        if (isWalkSFXRunning && !audioSource.isPlaying)
            PlayWalkSFX();

        if (isWallSlidingSFXRunning && !audioSource.isPlaying)
            PlayWallSlidingSFX();
    }
    private void PlaySFXOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
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
        int n = Random.Range(0, waterWalkSFX.Length);
        float randomPitch = Random.Range(lowPitchRange,highPitchRange);
        audioSource.pitch = randomPitch;        

        PlaySFXSingle(waterWalkSFX[n]);
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpSFX()
    {
        PlaySFXOneShot(waterTakeOffJumpSFX);
    }
    private void PlayLandingJumpSFX()
    {
        PlaySFXOneShot(waterLandingJumpSFX);
    }
    #endregion
    #region Wall Sliding
    private void TriggerWallSlidingSFX()
    {
        isWallSlidingSFXRunning = true;
    }
    private void StopWallSlidingSFX()
    {
        audioSource.loop = false;
        StopSFX();

        isWallSlidingSFXRunning = false;
    }
    private void PlayWallSlidingSFX()
    {        
        audioSource.loop = true;
        PlaySFXSingle(wallSlidingSFX);
    }
    #endregion
    #region Wall Jump
    private void PlayWallJumpSFX()
    {
        PlaySFXOneShot(wallJumpSFX);
    }
    #endregion
    #region Acorn
    private void PlayEatAcornSFX()
    {
        PlaySFXOneShot(eatAcornSFX);
    }
    #endregion
}
