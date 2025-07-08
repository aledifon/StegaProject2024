using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerVFX : MonoBehaviour
{    
    PlayerMovement playerMovement;
    PlayerHealth playerHealth;  
    [SerializeField] Transform originPS;
    [SerializeField] Transform originWallSlidePS;
    SpriteRenderer spriteRenderer;

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
    private bool isWallSlidingVFXRunning;

    [Header("Wall Jumping")]
    [SerializeField] private GameObject wallJumpVFX;
    private ParticleSystem wallJumpPS;

    [Header("Hook Thrown")]
    [SerializeField] private GameObject hookThrownVFX;
    private ParticleSystem hookThrownPS;

    [Header("Damage Fading")]
    [SerializeField] private float fadingOutTimer;
    [SerializeField] private float fadeOutDuration;             // 0.1f
    [SerializeField] private float fadingTimer;
    public float FadingTimer => fadingTimer;
    [SerializeField] private float fadingTotalDuration;         // 1.5f
    public float FadingTotalDuration => fadingTotalDuration;

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
        playerHealth = GetComponent<PlayerHealth>();    
        spriteRenderer = GetComponent<SpriteRenderer>();

        waterWalkPS = InstantiateVFXPrefabs(waterWalkVFX, originPS, transform);
        dustWalkPS = InstantiateVFXPrefabs(dustWalkVFX, originPS, transform);

        waterTakeOffJumpPS = InstantiateVFXPrefabs(waterTakeOffJumpVFX, originPS, transform);
        dustTakeOffJumpPS = InstantiateVFXPrefabs(dustTakeOffJumpVFX, originPS, transform);
        waterLandingJumpPS = InstantiateVFXPrefabs(waterLandingJumpVFX, originPS, transform);
        dustLandingJumpPS = InstantiateVFXPrefabs(dustLandingJumpVFX, originPS, transform);

        wallSlidingPS = InstantiateVFXPrefabs(wallSlidingVFX, originWallSlidePS, transform);
        wallJumpPS = InstantiateVFXPrefabs(wallJumpVFX, originPS, transform);
        
        hookThrownPS = InstantiateVFXPrefabs(hookThrownVFX, originPS, transform);        

        // Get the current landingJumpPS Local Pos. & Rot.
        waterLandingJumpPS.transform.localPosition += Vector3.down * playerMovement.RayLength;
        dustLandingJumpPS.transform.localPosition += Vector3.down * playerMovement.RayLength;                

        //Debug.Log("DustTakeOff LocalPos = " + dustTakeOffJumpPS.transform.localPosition);
        //Debug.Log("DustTakeOff LocalRot = " + dustTakeOffJumpPS.transform.localRotation);
        
        //Debug.Log("DustLanding LocalPos = " + dustLandingJumpPS.transform.localPosition);
        //Debug.Log("DustLanding LocalRot = " + dustLandingJumpPS.transform.localRotation);
        
        //Debug.Log("WallJump LocalPos = " + wallJumpPS.transform.localPosition);
        //Debug.Log("WallJump LocalRot = " + wallJumpPS.transform.localRotation);
    }    
    private void OnEnable()
    {
        // Walk
        playerMovement.OnStartWalking += PlayWalkVFX;
        playerMovement.OnStopWalking += StopWalkVFX;

        // Jump
        playerMovement.OnTakeOffJump += PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump += PlayLandingJumpVFX;

        // Wall Sliding
        playerMovement.OnStartWallSliding += PlayWallSlidingVFX;
        playerMovement.OnStopWallSliding += StopWallSlidingVFX;

        // Wall Jump
        playerMovement.OnWallJump += PlayWallJumpVFX;

        // Grappling-Hook
        playerMovement.OnStopRopeSwinging += PlayTakeOffJumpVFX;
        playerMovement.OnHookThrown += PlayHookThrownVFX;

        // Damage Player
        playerHealth.OnHitFXPlayer += TriggerSpriteFading;
    }
    private void OnDisable()
    {
        // Walk
        playerMovement.OnStartWalking -= PlayWalkVFX;
        playerMovement.OnStopWalking -= StopWalkVFX;

        // Jump
        playerMovement.OnTakeOffJump -= PlayTakeOffJumpVFX;
        playerMovement.OnLandingJump -= PlayLandingJumpVFX;

        // Wall Sliding
        playerMovement.OnStartWallSliding -= PlayWallSlidingVFX;
        playerMovement.OnStopWallSliding -= StopWallSlidingVFX;

        // Wall Jump
        playerMovement.OnWallJump -= PlayWallJumpVFX;

        // Grappling-Hook
        playerMovement.OnStopRopeSwinging -= PlayTakeOffJumpVFX;
        playerMovement.OnHookThrown -= PlayHookThrownVFX;

        // Damage
        playerHealth.OnHitFXPlayer -= TriggerSpriteFading;
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
        ParticleSystem ps = Instantiate(prefab, parentTransform).GetComponent<ParticleSystem>();
        ps.transform.localPosition = originTransform.localPosition;
        ps.transform.rotation = prefab.transform.rotation;

        return ps;

        //return Instantiate(prefab, originTransform.position, originTransform.rotation, parentTransform).
        //                GetComponent<ParticleSystem>();        
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

        //Debug.Log("Walking VFX Started");
    }
    private void StopWalkVFX()
    {
        if(GameManager.Instance.IsWetSurface)
            StopVFX(waterWalkPS);
        else
            StopVFX(dustWalkPS);

        isWalkVFXRunning = false;

        //Debug.Log("Walking VFX Stopped");
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
        //Debug.Log("Take Off Jump VFX Started");
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
        //Debug.Log("Landing Jump VFX Started");
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
        ResetParentOfWallSlidingPS();

        // Set the corresponding WallSlidingVFX Direction
        SetWallSlidingVFXDirection();

        PlayVFX(wallSlidingPS);        

        //isWallSlidingVFXRunning = true;

        //Debug.Log("Wall Sliding VFX Started");
    }
    private void StopWallSlidingVFX()
    {
        StopVFX(wallSlidingPS);

        // Clear the Player as parent of the PS to show it properly
        wallSlidingPS.transform.parent = null;

        //isWallSlidingVFXRunning = false;

        //Debug.Log("Wall Sliding VFX Stopped");
    }
    private void SetWallSlidingVFXDirection()
    {
        // Set Local Position
        Vector3 wallSlidePSLocalPos = originWallSlidePS.transform.localPosition;
        wallSlidingPS.transform.localPosition = playerMovement.SpriteRendPlayerFlipX ?
                        new Vector3(-wallSlidePSLocalPos.x, wallSlidePSLocalPos.y, wallSlidePSLocalPos.z) :
                        wallSlidePSLocalPos;

        // Set Local Rotation
        wallSlidingPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }
    private void ResetParentOfWallSlidingPS()
    {
        if (wallSlidingPS.transform.parent != transform)
        {
            wallSlidingPS.transform.SetParent(transform, false);

            // Set again the WallJumpingVFX Local pos & rot.
            wallSlidingPS.transform.localPosition = originWallSlidePS.localPosition;
            wallSlidingPS.transform.localRotation = originWallSlidePS.localRotation;
        }
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

        //Debug.Log("Wall Jump VFX Stopped");
    }
    private void SetWallJumpPSRot()
    {
        float zLocalRot = wallJumpPS.transform.localEulerAngles.z;
        wallJumpPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, zLocalRot) : 
                                                Quaternion.Euler(0, 0, zLocalRot);
    }
    private void ResetParentOfWallJumpPS()
    {
        if (wallJumpPS.transform.parent != transform)
        {
            wallJumpPS.transform.SetParent(transform,true);

            // Set again the WallJumpingVFX Local pos & rot.
            wallJumpPS.transform.localPosition = originPS.localPosition;
            //wallJumpPS.transform.localRotation = originPS.localRotation;
        }        
    }
    #endregion
    #region Hook Thrown
    private void PlayHookThrownVFX()
    {
        ResetParentOfHookThrownPS();

        // Set the corresponding hookThrownVFX Rotation
        SetHookThrownPSRot();

        PlayVFX(hookThrownPS);

        // Clear the Player as parent of the PS to show it properly
        //hookThrownPS.transform.parent = null;

        //Debug.Log(Hook Thrown VFX Stopped");
    }
    private void SetHookThrownPSRot()
    {        
        float zLocalRot = hookThrownPS.transform.localEulerAngles.z;
        hookThrownPS.transform.localRotation = playerMovement.SpriteRendPlayerFlipX ?
                                                Quaternion.Euler(0, 180, zLocalRot) :
                                                Quaternion.Euler(0, 0, zLocalRot);
    }
    private void ResetParentOfHookThrownPS()
    {
        if (hookThrownPS.transform.parent != transform)
        {
            hookThrownPS.transform.SetParent(transform, true);

            // Set again the hookThrownVFX Local pos & rot.
            hookThrownPS.transform.localPosition = originPS.localPosition;
            //hookThrownPS.transform.localRotation = originPS.localRotation;
        }
    }
    #endregion

    #region Damage
    private void TriggerSpriteFading(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        StartCoroutine(nameof(SpriteFading));
    }
    private IEnumerator SpriteFading()
    {
        Color targetColor = spriteRenderer.color;
        targetColor.a = 0f;

        fadingTimer = 0f;

        while (fadingTimer < fadingTotalDuration)
        {
            // Reset the Timer
            fadingOutTimer = 0f;

            // Inverse the Alpha Channel of the Target Color
            if (spriteRenderer.color == targetColor)
                targetColor.a = targetColor.a == 0f ? 1f : 0f;

            // Set the Start Color
            Color startColor = spriteRenderer.color;


            while (fadingOutTimer < fadeOutDuration)
            {
                // Color fading
                float t = fadingOutTimer / fadeOutDuration;
                spriteRenderer.color = Color.Lerp(startColor, targetColor, t);

                // Timers increment
                float delta = Time.unscaledDeltaTime;
                fadingOutTimer += delta;
                fadingTimer += delta;

                yield return null;
            }
            spriteRenderer.color = targetColor;
        }

        // Assure the the Sprite Renderer is visible when finish the Coroutine
        targetColor.a = 1f;
        spriteRenderer.color = targetColor;
    }
    #endregion
    #endregion
}
