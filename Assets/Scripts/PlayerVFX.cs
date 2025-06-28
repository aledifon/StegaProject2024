using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerVFX : MonoBehaviour
{    
    PlayerMovement playerMovement;
    [SerializeField] Transform originPS;

    [Header("Walk")]
    [SerializeField] private GameObject waterWalkVFX;
    //[SerializeField] private GameObject dustWalkVFX;
    private ParticleSystem waterWalkPS;
    //private ParticleSystem dustWalkPS;
    private bool isWalkVFXRunning;

    [Header("Jump")]
    [SerializeField] private GameObject waterTakeOffJumpVFX;
    //[SerializeField] private GameObject dustTakeOffJumpVFX;    
    private ParticleSystem waterTakeOffJumpPS;
    //private ParticleSystem dustTakeOffJumpPS;    

    [SerializeField] private GameObject waterLandingJumpVFX;
    //[SerializeField] private GameObject dustLandingJumpVFX;    
    private ParticleSystem waterLandingJumpPS;
    //private ParticleSystem dustLandingJumpPS;    

    [Header("Wall Sliding")]
    [SerializeField] private GameObject wallSlidingVFX;
    private ParticleSystem wallSlidingPS;    

    [Header("Wall Jumping")]
    [SerializeField] private GameObject wallJumpVFX;
    private ParticleSystem wallJumpPS;

    // Used for keeping pos. of TakeOffJumping VFX
    private Vector3 localPosWaterTakeOffJumpingVFX;
    private Quaternion localRotWaterTakeOffJumpingVFX;

    #region Unity API
    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();

        waterWalkPS = InstantiateVFXPrefabs(waterWalkVFX, originPS, transform);

        waterTakeOffJumpPS = InstantiateVFXPrefabs(waterTakeOffJumpVFX, originPS, transform);
        waterLandingJumpPS = InstantiateVFXPrefabs(waterLandingJumpVFX, originPS, transform);

        //wallSlidingPS = InstantiateVFXPrefabs(wallSlidingVFX, originPS, transform);
        //wallJumpPS = InstantiateVFXPrefabs(wallJumpVFX, originPS, transform);
    }    
    private void OnEnable()
    {
        playerMovement.OnStartWalking += PlayWalkVFX;
        playerMovement.OnStopWalking += StopWalkVFX;

        playerMovement.OnTakeOffJump += PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump += PlayLandingJumpVFX;

        //playerMovement.OnStartWallSliding += PlayWallSlidingVFX;
        //playerMovement.OnStopWallSliding += StopWallSlidingVFX;

        //playerMovement.OnWallJump += PlayWallJumpVFX;
    }
    private void OnDisable()
    {
        playerMovement.OnStartWalking -= PlayWalkVFX;
        playerMovement.OnStopWalking -= StopWalkVFX;

        playerMovement.OnTakeOffJump -= PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump -= PlayLandingJumpVFX;

        //playerMovement.OnStartWallSliding -= PlayWallSlidingVFX;
        //playerMovement.OnStopWallSliding -= StopWallSlidingVFX;

        //playerMovement.OnWallJump -= PlayWallJumpVFX;
    }
    private void Update()
    {
        if (isWalkVFXRunning)
            UpdateWalkVFXDirection();        
    }
    #endregion
    #region Private Methods
    
    //private void UpdateWalkVFX()
    //{
    //    UpdateWalkVFXDirection();

    //    if (playerMovement.IsGrounded &&
    //        playerMovement.CurrentState == PlayerMovement.PlayerState.Running &&
    //        !isWalkVFXRunning)
    //    {            
    //        PlayWalkVFX();
    //    }
    //    else if ((!playerMovement.IsGrounded || 
    //        playerMovement.CurrentState != PlayerMovement.PlayerState.Running) &&
    //        isWalkVFXRunning)
    //    {
    //        StopWalkVFX();
    //    }
    //}    
    private ParticleSystem InstantiateVFXPrefabs(GameObject prefab, Transform originTransform, Transform parentTransform)
    {
        return Instantiate(prefab, originTransform.position, originTransform.rotation, parentTransform).
                        GetComponent<ParticleSystem>();        
    }    
    private void PlayVFX(ParticleSystem ps)
    {
        if (!ps.isPlaying)
            ps.Play();
    }
    private void StopVFX(ParticleSystem ps)
    {
        if (ps.isPlaying)
            ps.Stop();
    }
    #region Walk
    private void PlayWalkVFX()
    {        
        PlayVFX(waterWalkPS);
        //PlayVFX(dustWalkPS);        
        isWalkVFXRunning = true;

        Debug.Log("Walking VFX Started");
    }
    private void StopWalkVFX()
    {
        StopVFX(waterWalkPS);
        //StopVFX(dustWalkPS);    
        isWalkVFXRunning = false;

        Debug.Log("Walking VFX Stopped");
    }
    private void UpdateWalkVFXDirection()
    {
        waterWalkPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpVFX()
    {
        // Save the current Local Position 
        localPosWaterTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localPosition;
        localRotWaterTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localRotation;

        // Clear the Player as parent of the PS to show it properly
        waterTakeOffJumpPS.transform.parent = null;
        PlayVFX(waterTakeOffJumpPS);
        //PlayVFX(dustTakeOffJumpPS);

        StartCoroutine(nameof(ResetParentOfTakeOffJumpPS));

        Debug.Log("Take Off Jump VFX Started");
    }
    IEnumerator ResetParentOfTakeOffJumpPS() 
    {
        // Espera hasta que el sistema esté realmente reproduciendo
        yield return new WaitForSeconds(0.5f);

        waterTakeOffJumpPS.transform.SetParent(transform);

        // Set again the local pos & rot.
        waterTakeOffJumpPS.transform.localPosition = localPosWaterTakeOffJumpingVFX;
        waterTakeOffJumpPS.transform.localRotation = localRotWaterTakeOffJumpingVFX;
    }
    private void PlayLandingJumpVFX()
    {        
        PlayVFX(waterLandingJumpPS);
        //PlayVFX(dustLandingJumpPS);

        Debug.Log("Landing Jump VFX Started");
    }
    #endregion
    #region Wall Sliding
    private void PlayWallSlidingVFX()
    {
        PlayVFX(wallSlidingPS);       
        
        Debug.Log("Wall Sliding VFX Started");
    }
    private void StopWallSlidingVFX()
    {
        StopVFX(wallSlidingPS);        

        Debug.Log("Wall Sliding VFX Stopped");
    }
    #endregion
    #region Wall Jump
    private void PlayWallJumpVFX()
    {
        PlayVFX(wallJumpPS);

        Debug.Log("Wall Jump VFX Stopped");
    }
    #endregion
    #endregion
}
