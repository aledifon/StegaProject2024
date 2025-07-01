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

    // Used for keeping pos. of LandingJumping VFX
    private Vector3 localPosLandingJumpingVFX;
    private Quaternion localRotLandingJumpingVFX;

    // Used for keeping pos. of WallJumping VFX
    private Vector3 localPosWallJumpingVFX;
    private Quaternion localRotWallJumpingVFX;

    // Coroutines
    Coroutine resetTakeOffJumpPSCoroutine;
    Coroutine resetWallJumpPSCoroutine;

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

        //// Get the current takeOffJumpPS Local Pos. & Rot.
        //localPosTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localPosition;
        //localRotTakeOffJumpingVFX = waterTakeOffJumpPS.transform.localRotation;

        // Get the current landingJumpPS Local Pos. & Rot.
        waterLandingJumpPS.transform.localPosition += Vector3.down * playerMovement.RayLength;
        dustLandingJumpPS.transform.localPosition += Vector3.down * playerMovement.RayLength;
        //localRotLandingJumpingVFX = waterLandingJumpPS.transform.localRotation;

        //// Get the current WalJumpingPS Local Pos. & Rot.
        //localPosWallJumpingVFX = wallJumpPS.transform.localPosition;
        //localRotWallJumpingVFX = wallJumpPS.transform.localRotation;

        Debug.Log("DustTakeOff LocalPos = " + dustTakeOffJumpPS.transform.localPosition);
        Debug.Log("DustTakeOff LocalRot = " + dustTakeOffJumpPS.transform.localRotation);
        
        Debug.Log("DustLanding LocalPos = " + dustLandingJumpPS.transform.localPosition);
        Debug.Log("DustLanding LocalRot = " + dustLandingJumpPS.transform.localRotation);
        
        Debug.Log("WallJump LocalPos = " + wallJumpPS.transform.localPosition);
        Debug.Log("WallJump LocalRot = " + wallJumpPS.transform.localRotation);
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
        ResetParentOfTakeOffJumpPS();           

        // Save the current Local Position 
        if (GameManager.Instance.IsWetSurface)
        {
            PlayVFX(waterTakeOffJumpPS);

            // Clear the Player as parent of the PS to show it properly
            waterTakeOffJumpPS.transform.parent = null;            
        }
        else
        {
            PlayVFX(dustTakeOffJumpPS);

            // Clear the Player as parent of the PS to show it properly
            dustTakeOffJumpPS.transform.parent = null;            
        }        
        Debug.Log("Take Off Jump VFX Started");
    }
    private void ResetParentOfTakeOffJumpPS()
    {        
        if (GameManager.Instance.IsWetSurface)
        {
            if (waterTakeOffJumpPS.transform.parent != transform)
            {                
                waterTakeOffJumpPS.transform.SetParent(transform, false);

                // Set again the local pos & rot.
                waterTakeOffJumpPS.transform.localPosition = originPS.localPosition;
                waterTakeOffJumpPS.transform.localRotation = originPS.localRotation;
            }
        }
        else
        {
            if (dustTakeOffJumpPS.transform.parent != transform)
            {                
                dustTakeOffJumpPS.transform.SetParent(transform, false);

                // Set again the local pos & rot.
                dustTakeOffJumpPS.transform.localPosition = originPS.localPosition;
                dustTakeOffJumpPS.transform.localRotation = originPS.localRotation;
            }            
        }
    }    
    private void PlayLandingJumpVFX()
    {
        ResetParentOfLandingJumpPS();

        if (GameManager.Instance.IsWetSurface)
        {
            PlayVFX(waterLandingJumpPS);

            // Clear the Player as parent of the PS to show it properly
            waterLandingJumpPS.transform.parent = null;            
        }
        else
        {
            PlayVFX(dustLandingJumpPS);

            // Clear the Player as parent of the PS to show it properly
            dustLandingJumpPS.transform.parent = null;            
        }            
        Debug.Log("Landing Jump VFX Started");
    }
    private void ResetParentOfLandingJumpPS()
    {
        if (GameManager.Instance.IsWetSurface)
        {
            if (waterLandingJumpPS.transform.parent != transform)
            {                
                waterLandingJumpPS.transform.SetParent(transform, false);

                // Set again the local pos & rot.
                waterLandingJumpPS.transform.localPosition = originPS.localPosition + 
                                                        (Vector3.down * playerMovement.RayLength);
                waterLandingJumpPS.transform.localRotation = originPS.localRotation;
            }
        }
        else
        {
            if (dustLandingJumpPS.transform.parent != transform)
            {                
                dustLandingJumpPS.transform.SetParent(transform, false);

                // Set again the local pos & rot.
                dustLandingJumpPS.transform.localPosition = originPS.localPosition + 
                                                        (Vector3.down * playerMovement.RayLength);
                dustLandingJumpPS.transform.localRotation = originPS.localRotation;
            }
        }
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
        ResetParentOfWallJumpPS();

        // Set the corresponding WallJumpingVFX Rotation
        SetWallJumpPSRot();

        PlayVFX(wallJumpPS);

        // Clear the Player as parent of the PS to show it properly
        wallJumpPS.transform.parent = null;                

        Debug.Log("Wall Jump VFX Stopped");
    }
    private void SetWallJumpPSRot()
    {
        wallJumpPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }
    private void ResetParentOfWallJumpPS()
    {
        if (wallJumpPS.transform.parent != transform)
        {
            wallJumpPS.transform.SetParent(transform,false);

            // Set again the WallJumpingVFX Local pos & rot.
            wallJumpPS.transform.localPosition = originPS.localPosition;
            wallJumpPS.transform.localRotation = originPS.localRotation;
        }        
    }    
    #endregion
    #endregion
}
