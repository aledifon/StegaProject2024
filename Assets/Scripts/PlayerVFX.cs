using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerVFX : MonoBehaviour
{    
    PlayerMovement playerMovement;
    [SerializeField] Transform originPS;

    [Header("Walk")]
    [SerializeField] private GameObject waterWalkVFX;
    [SerializeField] private GameObject dustWalkVFX;
    private ParticleSystem waterWalkPS;
    private ParticleSystem dustWalkPS;
    private bool isWalkVFXRunning;

    [Header("Jump")]
    [SerializeField] private GameObject waterTakeOffJumpVFX;
    [SerializeField] private GameObject dustTakeOffJumpVFX;
    private ParticleSystem waterTakeOffJumpPS;
    private ParticleSystem dustTakeOffJumpPS;

    [SerializeField] private GameObject waterLandingJumpVFX;
    [SerializeField] private GameObject dustLandingJumpVFX;
    private ParticleSystem waterLandingJumpPS;
    private ParticleSystem dustLandingJumpPS;

    [Header("Wall Sliding")]
    [SerializeField] private GameObject wallSlidingVFX;
    private ParticleSystem wallSlidingPS;    

    [Header("Wall Jumping")]
    [SerializeField] private GameObject wallJumpVFX;
    private ParticleSystem wallJumpPS;    

    // Used for keeping pos. of TakeOffJumping VFX
    private Vector3 localPosTakeOffJumpingVFX;
    private Quaternion localRotTakeOffJumpingVFX;

    // Used for keeping pos. of WallJumping VFX
    private Vector3 localPosWallJumpingVFX;
    private Quaternion localRotWallJumpingVFX;

    #region Unity API
    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();

        waterWalkPS = InstantiateVFXPrefabs(waterWalkVFX, originPS, transform);
        dustWalkPS = InstantiateVFXPrefabs(dustWalkVFX, originPS, transform);

        waterTakeOffJumpPS = InstantiateVFXPrefabs(waterTakeOffJumpVFX, originPS, transform);
        dustTakeOffJumpPS = InstantiateVFXPrefabs(dustTakeOffJumpVFX, originPS, transform);
        waterLandingJumpPS = InstantiateVFXPrefabs(waterLandingJumpVFX, originPS, transform);
        dustLandingJumpPS = InstantiateVFXPrefabs(dustLandingJumpVFX, originPS, transform);

        //wallSlidingPS = InstantiateVFXPrefabs(wallSlidingVFX, originPS, transform);
        wallJumpPS = InstantiateVFXPrefabs(wallJumpVFX, originPS, transform);
    }    
    private void OnEnable()
    {
        playerMovement.OnStartWalking += PlayWalkVFX;
        playerMovement.OnStopWalking += StopWalkVFX;

        playerMovement.OnTakeOffJump += PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump += PlayLandingJumpVFX;

        //playerMovement.OnStartWallSliding += PlayWallSlidingVFX;
        //playerMovement.OnStopWallSliding += StopWallSlidingVFX;

        playerMovement.OnWallJump += PlayWallJumpVFX;
    }
    private void OnDisable()
    {
        playerMovement.OnStartWalking -= PlayWalkVFX;
        playerMovement.OnStopWalking -= StopWalkVFX;

        playerMovement.OnTakeOffJump -= PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump -= PlayLandingJumpVFX;

        //playerMovement.OnStartWallSliding -= PlayWallSlidingVFX;
        //playerMovement.OnStopWallSliding -= StopWallSlidingVFX;

        playerMovement.OnWallJump -= PlayWallJumpVFX;
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
        //if (!ps.isPlaying)
        //    ps.Play();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        if(GameManager.Instance.IsWetSurface)
            PlayVFX(waterWalkPS);
        else
            PlayVFX(dustWalkPS);

        isWalkVFXRunning = true;

        Debug.Log("Walking VFX Started");
    }
    private void StopWalkVFX()
    {
        if(GameManager.Instance.IsWetSurface)
            StopVFX(waterWalkPS);
        else
            StopVFX(dustWalkPS);

        isWalkVFXRunning = false;

        Debug.Log("Walking VFX Stopped");
    }
    private void UpdateWalkVFXDirection()
    {
        if(GameManager.Instance.IsWetSurface)
            waterWalkPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        else
            dustWalkPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }
    #endregion
    #region Jump
    private void PlayTakeOffJumpVFX()
    {
        // Save the current Local Position 
        if (GameManager.Instance.IsWetSurface)
        {
            localPosTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localPosition;
            localRotTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localRotation;

            // Clear the Player as parent of the PS to show it properly
            waterTakeOffJumpPS.transform.parent = null;
            PlayVFX(waterTakeOffJumpPS);
        }
        else
        {
            localPosTakeOffJumpingVFX = dustTakeOffJumpPS.transform.localPosition;
            localRotTakeOffJumpingVFX = dustTakeOffJumpPS.transform.localRotation;

            // Clear the Player as parent of the PS to show it properly
            dustTakeOffJumpPS.transform.parent = null;
            PlayVFX(dustTakeOffJumpPS);
        }               

        StartCoroutine(nameof(ResetParentOfTakeOffJumpPS));

        Debug.Log("Take Off Jump VFX Started");
    }
    IEnumerator ResetParentOfTakeOffJumpPS() 
    {
        // Espera hasta que el sistema esté realmente reproduciendo
        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.IsWetSurface)
        {
            waterTakeOffJumpPS.transform.SetParent(transform);

            // Set again the local pos & rot.
            waterTakeOffJumpPS.transform.localPosition = localPosTakeOffJumpingVFX;
            waterTakeOffJumpPS.transform.localRotation = localRotTakeOffJumpingVFX;
        }
        else
        {
            dustTakeOffJumpPS.transform.SetParent(transform);

            // Set again the local pos & rot.
            dustTakeOffJumpPS.transform.localPosition = localPosTakeOffJumpingVFX;
            dustTakeOffJumpPS.transform.localRotation = localRotTakeOffJumpingVFX;
        }
    }
    private void PlayLandingJumpVFX()
    {        
        if (GameManager.Instance.IsWetSurface)
            PlayVFX(waterLandingJumpPS);
        else
            PlayVFX(dustLandingJumpPS);

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
        localPosWallJumpingVFX = wallJumpPS.transform.localPosition;
        localRotWallJumpingVFX = wallJumpPS.transform.localRotation;

        // Clear the Player as parent of the PS to show it properly
        wallJumpPS.transform.parent = null;

        PlayVFX(wallJumpPS);

        StartCoroutine(nameof(ResetParentOfWallJumpPS));

        Debug.Log("Wall Jump VFX Stopped");
    }
    IEnumerator ResetParentOfWallJumpPS()
    {
        // Espera hasta que el sistema esté realmente reproduciendo
        yield return new WaitForSeconds(0.5f);

        wallJumpPS.transform.SetParent(transform);

        // Set again the local pos & rot.
        wallJumpPS.transform.localPosition = localPosWallJumpingVFX;
        wallJumpPS.transform.localRotation = localRotWallJumpingVFX;        
    }
    #endregion
    #endregion
}
