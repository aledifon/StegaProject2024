using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    //private static PlayerVFXManager instance;
    //public static PlayerVFXManager Instance
    //{
    //    get
    //    {
    //        if (instance == null)
    //        {
    //            instance = FindAnyObjectByType<PlayerVFXManager>();

    //            if (instance == null)
    //            {
    //                GameObject go = new GameObject("GameManager");
    //                instance = go.AddComponent<PlayerVFXManager>();
    //            }
    //        }
    //        return instance;
    //    }
    //}

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
    private bool isTakeOffJumpVFXRunning;

    [SerializeField] private GameObject waterLandingJumpVFX;
    //[SerializeField] private GameObject dustLandingJumpVFX;    
    private ParticleSystem waterLandingJumpPS;
    //private ParticleSystem dustLandingJumpPS;
    private bool isLandingJumpVFXRunning;

    //[Header("Wall Jumping")]
    //[SerializeField] private GameObject wallSlideVFX;
    //private ParticleSystem wallSlidePS;
    //private bool isWallSlideVFXRunning;

    //[SerializeField] private GameObject wallJumpVFX;
    //private ParticleSystem wallJumpPS;
    //private bool isWallJumpVFXRunning;

    #region Unity API
    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();

        waterWalkPS = InstantiateVFXPrefabs(waterWalkVFX, originPS, transform);

        waterTakeOffJumpPS = InstantiateVFXPrefabs(waterTakeOffJumpVFX, originPS, transform);
        waterLandingJumpPS = InstantiateVFXPrefabs(waterLandingJumpVFX, originPS, transform);

        //wallSlidePS = InstantiateVFXPrefabs(wallSlideVFX, originPS, transform);
        //wallJumpPS = InstantiateVFXPrefabs(wallJumpVFX, originPS, transform);
    }
    private void Update()
    {

        UpdateWalkVFX();
        //UpdateTakeOffJumpVFX();
        //UpdateLandingJumpVFX();
        //UpdateWallSlideVFX();
        //UpdateWallJumpVFX();
    }    
    #endregion
    #region Private Methods
    #region VFX Private Methods
    private void UpdateWalkVFX()
    {
        UpdateWalkVFXDirection();

        if (playerMovement.IsGrounded &&
            playerMovement.CurrentState == PlayerMovement.PlayerState.Running &&
            !isWalkVFXRunning)
        {            
            PlayWalkVFX();
        }
        else if ((!playerMovement.IsGrounded || 
            playerMovement.CurrentState != PlayerMovement.PlayerState.Running) &&
            isWalkVFXRunning)
        {
            StopWalkVFX();
        }
    }
    private void UpdateWalkVFXDirection()
    {
        waterWalkPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ? 
                                                Quaternion.Euler(0,180,0) : Quaternion.identity;
    }
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
    #endregion
    #endregion

    #region VFX Public Methods

    #region VFX Play Methods
    public void PlayWalkVFX()
    {        
        PlayVFX(waterWalkPS);
        //PlayVFX(dustWalkPS);

        isWalkVFXRunning = true;

        Debug.Log("Walking VFX Started");
    }
    public void PlayTakeOffJumpVFX()
    {        
        PlayVFX(waterTakeOffJumpPS);
        //PlayVFX(dustTakeOffJumpPS);

        isTakeOffJumpVFXRunning = true;
    }
    public void PlayLandingJumpVFX()
    {        
        PlayVFX(waterLandingJumpPS);
        //PlayVFX(dustLandingJumpPS);

        isLandingJumpVFXRunning = true;
    }
    //public void PlayWallSlideVFX()
    //{
    //    PlayVFX(wallSlidePS);
    //
    //    isWallSlideVFXRunning = true; 
    //}
    //public void PlayWallJumpVFX()
    //{        
    //    PlayVFX(wallJumpPS);
    //    
    //    isWallJumpVFXRunning = true;
    //}
    #endregion
    #region VFX Stop Methods
    public void StopWalkVFX()
    {
        StopVFX(waterWalkPS);
        //StopVFX(dustWalkPS);    

        isWalkVFXRunning = false;

        Debug.Log("Walking VFX Stopped");
    }
    public void StopTakeOffJumpVFX()
    {
        StopVFX(waterTakeOffJumpPS);
        //StopVFX(dustTakeOffJumpPS);    

        isTakeOffJumpVFXRunning = false;
    }
    public void StopLandingJumpVFX()
    {
        StopVFX(waterLandingJumpPS);
        //StopVFX(dustLandingJumpPS);    

        isLandingJumpVFXRunning = false;
    }
    //public void StopWallSlideVFX()
    //{
    //    StopVFX(wallSlidePS);
    //    
    //    isWallSlideVFXRunning = false;
    //}
    //public void StopWallJumpVFX()
    //{
    //    StopVFX(wallJumpPS);        
    //
    //    isWallJumpVFXRunning = false;
    //}
    #endregion
    #endregion
}
