using UnityEngine.InputSystem;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour
{
    // Global vars
    [Header("Velocity")]
    [SerializeField] float speed;
    [SerializeField] float smoothTime; // time will takes to reach the speed.
    [SerializeField] float lerpSpeed;  // Interpolation Time
    [SerializeField] float smoothKeyboardSpeed; // Smoothing speed given to the movement when is
                                                // performed by the keyboard

    [Header("Jump")]
    [SerializeField] float jumpEnemyThrust;
    [SerializeField] float jumpVertSpeed;       // Jumping applied Force
    [SerializeField] private float jumpHorizSpeed;       // Jumping applied Force
    [SerializeField] bool jumpTriggered;    // Jumping applied Force
    [SerializeField] bool wallJumpTriggered;    // Jumping applied Force

    [SerializeField] float minJumpVertDist;     // Min. Vert. Jumping Dist. Uds.
    [SerializeField] float maxJumpVertDist;     // Max. Vert. Jumping Dist. Uds.    
    [SerializeField] float jumpingTimer;    // Jumping Timer
    private float minJumpingTime;           // Min & Max Jumping Times (in func. of Jumping distance. & Jump. speed)                                            
    private float maxJumpingTime;
    protected bool jumpPressed;    
    public bool JumpPressed => jumpPressed;       

    [SerializeField] private float maxJumpHorizDist;  // Max allowed Horizontal Jumping distance
    //[SerializeField] private float maxJumpHorizTimer; // Horizontal Jumping Timer                                                      
    [SerializeField] private float maxJumpHorizTime;         // Max Jumping time on horizontal Movement
                                                             // (Calculated in func. of maxJumpDistance & Player's speed)        

    [Header("Wall Jump")]
    [SerializeField] float wallJumpVertSpeed;           // Wall Jumping applied Speed                   || Previous value = 12f   
    private float wallJumpHorizSpeed;                   // Wall Jumping applied Speed    
    //[SerializeField] float wallJumpVertDist;          // Wall Jumping Vert. Dist. Uds.
    [SerializeField] private float wallJumpHorizDist;   // Max allowed Horizontal Wall Jumping distance  || Previous value = 3f   
    [SerializeField] private float wallJumpHorizDist2;   // Max allowed Horizontal Wall Jumping distance || Previous value = 4.5f   
                                                         // after the player takes the control again
    [SerializeField] private float wallJumpHorizTime;   // Max Jumping time on horizontal Movement       || Previous value = 0.6f   
    private float wallJumpingVertTime;                          // Wall Jumping Time (in func. of Wall Jumping distance. & Wall Jump. speed)                                                    
    Vector2 wallJumpSpeedVector;
    [SerializeField] private float wallJumpDelayMaxTime;    
    [SerializeField] private float wallJumpDelayTimer;    
    [SerializeField] private bool isWallJumpDelayEnabled;    
    [SerializeField] bool isWallJumpUnlocked;

    [Header("Hook")]
    [SerializeField] protected bool hookActionPressed;
    public bool HookActionPressed => hookActionPressed;
    [SerializeField] private float hookThrownMaxTime;
    public float HookThrownMaxTime => hookThrownMaxTime;
    [SerializeField] private float hookThrownTimer;
    public float HookThrownTimer => hookThrownTimer;
    [SerializeField] private bool isHookThrownEnabled;
    public bool IsHookThrownEnabled => isHookThrownEnabled;
    [SerializeField] bool isHookUnlocked;
    protected bool IsHookUnlocked => isHookUnlocked;

    [Header("Swinging")]
    [SerializeField] private float swingHorizForce;         // 0.8f    
    [SerializeField] private float opposeSwingForceFactor;  // 0.5f
    // Define min and max angles
    [SerializeField] float minSwingAngle;                   // 60f
    [SerializeField] float maxSwingAngle;                   // 140f;
    // Define max Swing Speed
    [SerializeField] float maxSwingSpeed;                   // = 100f;
    [SerializeField] float maxInitialSwingSpeed;            // 20f;
    [SerializeField] bool isRopeSwinging;

    // Coyote Time vars
    [Header("Coyote Time")]
    [SerializeField] private float maxCoyoteTime;
    [SerializeField] private float coyoteTimer;
    [SerializeField] private bool isCoyoteTimerEnabled;

    // Raycast Corner checks
    [Header("Corner Detection")]
    [SerializeField] Transform cornerLeftCheck;   //Raycast origin point (Vitamini feet)
    [SerializeField] Transform cornerRightCheck;  //Raycast origin point (Vitamini feet)
    [SerializeField] float rayCornerLength;     //Raycast Corner Length
                                                //[SerializeField] bool cornerDetected;       //Corner detection flag

    // Input Buffer
    [Header("Input Buffer")]
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float jumpBufferTimer;
    [SerializeField] private bool isJumpBufferEnabled;

    [Header("Death")]
    [SerializeField] private float forceJumpDeath;

    // Enemy Hit
    [Header("Enemy Thrust")]
    [SerializeField] float thrustEnemyDuration;         // 0.08f for enemyMovement.thrustToPlayer = 25f;
    [SerializeField] float thrustEnemyTimer;
    [SerializeField] bool isThrustEnemyTimerEnabled;    
    private Vector2 enemyHitDir;
    private float enemyHitThrust;    

    // UI        
    [Header("Gems")]
    [SerializeField] private TextMeshProUGUI textGemsUI;
    private float numGems;
    private float NumGems
    { get { return numGems; } 
      set 
        {
            if (value >= 10)
            {
                numGems = 0;
                IncreaseLifes();
                // Play Get Life SFx            
                OnGetLife?.Invoke();
            }                
            else
                numGems = value; 
        } 
    }

    [Header("Lifes")]
    [SerializeField] private TextMeshProUGUI textLifesUI;
    private float numLifes;
    private float NumLifes
    { get { return numLifes; } 
      set { numLifes = Mathf.Clamp(value,0,99); } 
    }    

    [Header("Raycast")]
    // Raycast Ground check
    [SerializeField] Transform[] groundChecks;  //Raycast origin point (Vitamini feet)
    [SerializeField] LayerMask groundLayer;     //Ground Layer
    [SerializeField] float rayLength;           //Raycast Length
    public float RayLength => rayLength;
    [SerializeField] bool isGrounded;           //Ground touching flag
    [SerializeField] bool isJumping;            //Jumping/Falling/WallJumping Flag
    protected bool IsJumping => isJumping;            
    [SerializeField] private bool isRayGroundDetected;       // Aux. Ray Ground var
    [SerializeField] private bool isRecentlyJumping;           // Aux. var 
    public bool IsGrounded => isGrounded;
    private bool wasGrounded;               //isGrounded value of previous frame

    [Header("Raycast Walls")]
    [SerializeField] Transform wallLeftCheck;     //Raycast origin point 
    [SerializeField] Transform wallRightCheck;     //Raycast origin point 
    [SerializeField] LayerMask wallLayer;       //Wall Layer
    [SerializeField] float rayWallLength;    //Raycast Wall Fwd Length
    [SerializeField] bool isWallDetected;       //Wall Fwd detection flag        
    Vector2 rayWallOrigin;
    Vector2 rayWallDir;
    [SerializeField] private bool isRayWallDetected;       // Aux. Ray Wall var
    [SerializeField] private bool isRecentlyWallJumping;     // Aux. var 

    [Header("Platform")]
    [SerializeField] LayerMask platformLayer;

    [Header("Hurt")]
    [SerializeField] private bool isHurt;
    public bool IsHurt => isHurt;

    [Header("Key")]
    [SerializeField] private bool isKeyUnlocked;
    public bool IsKeyUnlocked => isKeyUnlocked;

    #region Enums    
    private enum CornerDetected
    {
        NoCeiling,
        Ceiling,
        CornerLeft,
        CornerRight,
    }    
    // Define Character States
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        WallJumping,
        Falling,
        WallBraking,
        Swinging,
        Hurting
    }
    [Header("Enums")]
    [SerializeField] private CornerDetected cornerDetected = CornerDetected.NoCeiling;
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    public PlayerState CurrentState => currentState;    
    #endregion

    #region Events & Delegates
    public event Action OnStartWalking;
    public event Action OnStopWalking;

    public event Action OnTakeOffJump;
    public event Action OnLandingJump;

    public event Action OnStartWallSliding;
    public event Action OnStopWallSliding;

    public event Action OnWallJump;
    public event Action OnStopAirSpin;

    public event Action OnHookThrown;
    public event Action OnHookRelease;    
    public event Action OnStartRopeSwinging;
    public event Action OnStopRopeSwinging;

    public event Action OnEatAcorn;    
    public event Action OnGetLife;    
    #endregion

    // Delay Time used for triggering OnLandingJump Event
    private float lastLandingTime;

    // GO Components
    protected Rigidbody2D rb2D;
    public float rb2DPlayerVelY => rb2D.linearVelocityY;
    public float rb2DPlayerVelX => rb2D.linearVelocityX;
    // Movement vars.
    protected float inputX;
    public float InputX => inputX;
    //Vector2 inputPlayerVelocity;    // Velocity given by the player's input
    Vector2 targetVelocity;          // Desired target player Speed(Velocity Movement type through rb2D)
    Vector2 dampVelocity;            // Player's current speed storage (Velocity Movement type through r
    Vector2 direction;              // To handle the direction with the New Input System
    Vector2 lastDirection;
    protected float inputDirDeadZone = 0.2f;  // dead Zone area to not take into account (to fix Analog Stick direction bugs)

    float rb2DDirVelX;    
    float rb2DJumpVelY;

    // GO Components
    SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRendPlayer => spriteRenderer;
    Animator animator;
    BoxCollider2D boxCollider2D;    
    PlayerVFX playerVFX;
    PlayerSFX playerSFX;
    PlayerHook playerHook;
    PlayerHealth playerHealth;    

    // Flip Flag
    //private bool lastFlipState;

    private bool isDead;
    public bool IsDead { get => isDead; set => isDead = value; }    

    public bool SpriteRendPlayerFlipX => spriteRenderer.flipX;    

    #region Unity API
    protected virtual void OnDrawGizmos()
    {
        if (Camera.current == null || Camera.current.name != "Main Camera") return;

        // Ground Raycasts Debugging
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundChecks[0].position, Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[0].position + (Vector3.right * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[0].position + (Vector3.right * 0.02f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[0].position + (Vector3.left * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[0].position + (Vector3.left * 0.02f), Vector2.down * rayLength);
        //Gizmos.color = Color.blue;
        Gizmos.DrawRay(groundChecks[1].position, Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[1].position + (Vector3.right * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[1].position + (Vector3.right * 0.02f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[1].position + (Vector3.left * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[1].position + (Vector3.left * 0.02f), Vector2.down * rayLength);
        //Gizmos.color = Color.green;
        Gizmos.DrawRay(groundChecks[2].position, Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[2].position + (Vector3.right * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[2].position + (Vector3.right * 0.02f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[2].position + (Vector3.left * 0.01f), Vector2.down * rayLength);
        Gizmos.DrawRay(groundChecks[2].position + (Vector3.left * 0.02f), Vector2.down * rayLength);

        // Ceiling Raycast Debugging
        Gizmos.color = Color.green;
        Gizmos.DrawRay(cornerLeftCheck.position, Vector2.up * rayCornerLength);
        Gizmos.DrawRay(cornerLeftCheck.position + (Vector3.right * 0.01f), Vector2.up * rayCornerLength);
        Gizmos.DrawRay(cornerLeftCheck.position + (Vector3.left * 0.01f), Vector2.up * rayCornerLength);
        Gizmos.DrawRay(cornerRightCheck.position, Vector2.up * rayCornerLength);
        Gizmos.DrawRay(cornerRightCheck.position + (Vector3.right * 0.01f), Vector2.up * rayCornerLength);
        Gizmos.DrawRay(cornerRightCheck.position + (Vector3.left * 0.01f), Vector2.up * rayCornerLength);

        // Ground Raycasts Debugging
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rayWallOrigin, rayWallDir * rayWallLength);
        Gizmos.DrawRay(rayWallOrigin + (Vector2.up * 0.01f), rayWallDir * rayWallLength);
        Gizmos.DrawRay(rayWallOrigin + (Vector2.up * 0.02f), rayWallDir * rayWallLength);
        Gizmos.DrawRay(rayWallOrigin + (Vector2.down * 0.01f), rayWallDir * rayWallLength);
        Gizmos.DrawRay(rayWallOrigin + (Vector2.down * 0.02f), rayWallDir * rayWallLength);
    }
    protected virtual void OnEnable()
    {
        // Subscription to PLayerMovement Events from the GameManager
        // (Need to be OnEnable to assure the GameManager is ready)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubscribeEventsOfPlayerMovement(this);
            GameManager.Instance.EnableReplayManagerAndGetRefs();
            GameManager.Instance.GetInputActionMaps(GetComponent<PlayerInput>().actions);
        }
        
        playerHealth.OnDeathPlayer += Death;                
    }
    protected virtual void OnDisable()
    {
        //if (GameManager.Instance != null)
        //    GameManager.Instance.OnHitPhysicsPlayer -= ReceiveDamage;
        playerHealth.OnDeathPlayer -= Death;                

        // Unsubscription to PLayerMovement Events from the GameManager
        // (Need to be OnDisable to assure clean the refs when switching Scenes)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DisableAllInputs();            // PENDING TO BE MANAGED FROM
                                                                // SCENE MANAGEMENT ON GameManager.cs!!
            GameManager.Instance.DisableReplayManagerAndCleanRefs();
            GameManager.Instance.UnsubscribeEventsOfPlayerMovement();
        }            
    }
    protected virtual void Awake()
    {
        // ONLY FOR RECORDING
        // Establecer c�mara lenta (por ejemplo, a la mitad de velocidad)
        //Time.timeScale = 0.3f;
        // ONLY FOR RECORDING
        
        GetGORefs();

        NumGems = 0;
        textGemsUI.text = NumGems.ToString();

        NumLifes = 0;
        textLifesUI.text = NumLifes.ToString();
        
        // Just for debugging
        //GameManager.Instance.EnableSlowMotion();

        //StartCoroutine(nameof(WaitForCameraAndSendRefs));
        SendRefsToCamera();
    }
    private void SendRefsToCamera()
    {
        CameraFollow cameraFollow = FindAnyObjectByType<CameraFollow>();                

        if (cameraFollow != null)
            cameraFollow.GetRefsOfPlayerMovement(this);
        else
            Debug.LogError("CameraFollow component Not Found on the Scene!");
    }
    private IEnumerator WaitForCameraAndSendRefs()
    {
        yield return new WaitUntil(() => Camera.main != null);

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
            cameraFollow.GetRefsOfPlayerMovement(this);
        else
            Debug.LogError("CameraFollow component Not Found on the Scene!");
    }
    protected virtual void Update()
    {                       
        // Update the Input player velocity        
        //inputPlayerVelocity = new Vector2(horizontal * speed, 0);
        // Update the target Velocity
        //targetVelocity += rb2D.velocity + inputPlayerVelocity;
                        
    }
    protected virtual void FixedUpdate()
    {
        // Last State vars Update
        wasGrounded = isGrounded;        

        // Launch the raycast to detect the ground
        RaycastGrounded();
        // Launch the raycast to detect the ceiling
        RaycastCeiling();
        // Launch the raycast to detect Vertical Walls
        RaycastVertWalls();
        // Controls if the player is on the elevator
        //Elevator();

        // Update the isGrounded, isWallDetected & isJumping Flags
        isGrounded = isRayGroundDetected && !isRecentlyJumping;
        isWallDetected = (isRayWallDetected && !isRecentlyWallJumping) && isWallJumpUnlocked;
        isJumping = (currentState == PlayerState.Jumping || 
                    currentState == PlayerState.Falling ||
                    currentState == PlayerState.WallJumping);
        isRopeSwinging = currentState == PlayerState.Swinging;

        // Jump Input Buffer
        CheckJumpTrigger();
        if (isJumpBufferEnabled)
            UdpateJumpBufferTimer();

        // Coyote Timer
        CheckCoyoteTimer();
        if (isCoyoteTimerEnabled)
            UdpateCoyoteTimer();

        // Hook Thrown
        if (isHookThrownEnabled)
            UdpateHookThrownTimer();

        // Wall Jump Delay Timer
        if (isWallJumpDelayEnabled)
            UdpateWallJumpDelayTimer();

        // Update the player state
        UpdatePlayerState();

        // Jumping Timer
        if (isJumping)        
            UpdateJumpTimer();

        // Thrust Enemy Timer
        if (isThrustEnemyTimerEnabled)
            UdpateThrustEnemyTimer();

        // Update the Horiz & Vertical Player's Speed
        UpdateHorizSpeed();
        UpdateVerticalSpeed();                    
        // Update the Player's Movement
        UpdatePlayerSpeed();

        // Update the player's gravity when falling down
        ChangeGravity();

        // Jumping Animation
        //AnimatingJumping();

        // Animations
        //UpdateAnimations();
        UpdateAnimationSpeed();

        UpdateInputAndSprite(direction);

        //Debug.Log($" inputX: {inputX}");
    }
    // Collisions
    //protected virtual void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.CompareTag("Ant"))
    //    {            
    //        AttackEnemy(collision.gameObject);
    //    }
    //    //else if (collision.collider.CompareTag("Acorn"))
    //    //{
    //    //    // Acorn dissappear
    //    //    Destroy(collision.collider.gameObject);
    //    //    // Increase Acorn counter
    //    //    NumAcorn++;
    //    //    // Update Acorn counter UI Text
    //    //    textAcornUI.text = NumAcorn.ToString();

    //    //    // Play Acorn Fx            
    //    //    OnEatAcorn?.Invoke();

    //    //    // Condition to pass to the next Scene
    //    //    //if (NumAcorn == 3)
    //    //    //    LoadScene();
    //    //}
    //}
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Acorn"))
        {
            // Acorn dissappear
            Destroy(collision.gameObject);
            // Increase Acorn counter
            NumGems++;
            // Update Acorn counter UI Text
            textGemsUI.text = NumGems.ToString();

            // Play Acorn Fx            
            OnEatAcorn?.Invoke();

            //// Condition to pass to the next Scene ()
            //if (NumAcorn == 30)
            //{
            //    LoadSceneAfterDelay();
            //}                
        }
        else if (collision.CompareTag("SuperAcorn"))
        {
            // Acorn dissappear
            Destroy(collision.gameObject);
            // Increase Acorn counter
            NumGems++;
            // Update Acorn counter UI Text
            textGemsUI.text = NumGems.ToString();

            // Play Finish Level Fx            
            //OnEatAcorn?.Invoke();
            GameManager.Instance.PlayEndOfLevelSFx();

            // Reload the Level after elapsed x seconds
            LoadSceneAfterDelay();            
        }
        //else if (collision.CompareTag("Ant"))
        //{
        //    AttackEnemy(collision.gameObject);
        //}
    }
    #endregion

    #region Get GO Refs
    protected void GetGORefs()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        playerVFX = GetComponent<PlayerVFX>();
        playerSFX = GetComponent<PlayerSFX>();
        playerHook = GetComponent<PlayerHook>();
        playerHealth = GetComponent<PlayerHealth>();
    }
    #endregion

    #region Player State
    // Player State
    private void UpdatePlayerState()
    {
        // Trigger to Hurting State from any State
        if (isHurt)
        {
            currentState = PlayerState.Hurting;
            UpdateAnimations();
            //Debug.Log("From " + currentState + " state to Hurting State. Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
        }
        else
        {
            switch (currentState)
            {
                case PlayerState.Hurting:
                    if (!isHurt)
                    {
                        currentState = PlayerState.Idle;
                        UpdateAnimations();
                    }                        
                    //Debug.Log("From Hurt state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case PlayerState.Idle:
                    if (isGrounded)
                    {
                        if (inputX != 0)
                        {
                            currentState = PlayerState.Running;
                            UpdateAnimations();
                            OnStartWalking?.Invoke();
                            //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                        }
                        //else if (!isGrounded && rb2D.velocity.y > 0)
                        else if (jumpTriggered)
                        {
                            TriggerJump();
                            OnTakeOffJump?.Invoke();            // Trigger Take Off Jump Event        
                            currentState = PlayerState.Jumping;
                            UpdateAnimations();

                            //Debug.Log("From Jumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                        }
                    }
                    else if (rb2D.linearVelocity.y < Mathf.Epsilon)
                    {
                        currentState = PlayerState.Falling;
                        UpdateAnimations();
                    }

                    //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case PlayerState.Running:
                    if (isGrounded)
                    {
                        if (inputX == 0)
                        {
                            OnStopWalking?.Invoke();        // Trigger Stop Walking Event
                            currentState = PlayerState.Idle;
                            UpdateAnimations();
                            //Debug.Log("From Running state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                        }
                        //else if (!isGrounded && rb2D.velocity.y > 0)
                        else if (jumpTriggered)
                        {
                            OnStopWalking?.Invoke();        // Trigger Stop Walking Event
                            TriggerJump();
                            OnTakeOffJump?.Invoke();        // Trigger Take Off Jump Event        

                            currentState = PlayerState.Jumping;
                            UpdateAnimations();
                            //Debug.Log("From Running state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                        }
                    }
                    else if (rb2D.linearVelocity.y < Mathf.Epsilon)
                    {
                        OnStopWalking?.Invoke();        // Trigger Stop Walking Event
                        currentState = PlayerState.Falling;
                        UpdateAnimations();
                    }
                    //Debug.Log("From Running state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case PlayerState.Jumping:
                    if (wallJumpTriggered)
                    {
                        if(playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        TriggerWallJump();
                        OnWallJump?.Invoke();           // Trigger Wall Jump Event
                        currentState = PlayerState.WallJumping;
                        UpdateAnimations();
                        //Debug.Log("From Jumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (playerHook.IsHookAttached)
                    {
                        if (playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        ResetPlayerSpeedBeforeSwinging();
                        OnStartRopeSwinging?.Invoke();
                        currentState = PlayerState.Swinging;
                        UpdateAnimations();

                        //Debug.Log("From Jumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (rb2D.linearVelocity.y < 0 && !isRecentlyJumping)
                    {
                        if (playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        currentState = PlayerState.Falling;
                        UpdateAnimations();
                        //Debug.Log("From Jumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case PlayerState.WallJumping:
                    if (rb2D.linearVelocity.y > 0 && inputX != 0  && !isWallJumpDelayEnabled)
                    {
                        // Wait for a some frames (wallJumpDelayMaxTime) to assure a proper wall Jump
                        currentState = PlayerState.Jumping;
                        UpdateAnimations();
                    }
                    else if (playerHook.IsHookAttached)
                    {
                        if (playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        ResetPlayerSpeedBeforeSwinging();
                        OnStartRopeSwinging?.Invoke();
                        currentState = PlayerState.Swinging;
                        UpdateAnimations();

                        //Debug.Log("From WallJumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (wallJumpTriggered)
                    {
                        if (playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        TriggerWallJump();
                        OnWallJump?.Invoke();               // Trigger Wall Jump Event
                        //currentState = PlayerState.WallJumping;
                        UpdateAnimations();
                    }
                    else if (rb2D.linearVelocity.y < 0 && !isRecentlyWallJumping)
                    {
                        if (playerSFX.IsAirSpinSFXRunning)
                            OnStopAirSpin?.Invoke();
                        currentState = PlayerState.Falling;
                        UpdateAnimations();
                    }
                    Debug.Log("From Wall Jumping state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case PlayerState.Falling:
                    if (jumpTriggered)
                    {
                        TriggerJump();
                        OnTakeOffJump?.Invoke();                // Trigger Take Off Jump Event        
                        currentState = PlayerState.Jumping;
                        UpdateAnimations();
                        //Debug.Log("From Falling state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (isGrounded)
                    {
                        //if (/*!jumpPressed &&*/ inputX == 0)
                        //    currentState = PlayerState.Idle;
                        //else if (/*!jumpPressed &&*/ inputX != 0)
                        //    currentState = PlayerState.Running;

                        if (Time.time - lastLandingTime > 0.1f)
                        {
                            lastLandingTime = Time.time;
                            OnLandingJump?.Invoke();                // Trigger Landing Jump Event        
                        }
                        currentState = PlayerState.Idle;
                        UpdateAnimations();

                        //Debug.Log("From Falling state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (playerHook.IsHookAttached)
                    {
                        ResetPlayerSpeedBeforeSwinging();
                        OnStartRopeSwinging?.Invoke();
                        currentState = PlayerState.Swinging;
                        UpdateAnimations();

                        //Debug.Log("From Falling state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (isWallDetected)
                    {
                        OnStartWallSliding?.Invoke();           // Trigger Start Wall Sliding Event        
                        currentState = PlayerState.WallBraking;
                        UpdateAnimations();
                        //Debug.Log("From Falling state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case PlayerState.WallBraking:
                    if (isGrounded)
                    {
                        //if (/*!jumpPressed &&*/ inputX == 0)
                        //    currentState = PlayerState.Idle;
                        //else if (/*!jumpPressed &&*/ inputX != 0)
                        //    currentState = PlayerState.Running;

                        OnStopWallSliding?.Invoke();            // Trigger Stop Wall Sliding Event
                        OnLandingJump?.Invoke();                // Trigger Landing Jump Event        
                        currentState = PlayerState.Idle;
                        UpdateAnimations();
                    }
                    else
                    {
                        if (!isWallDetected)
                        {
                            OnStopWallSliding?.Invoke();        // Trigger Stop Wall Sliding Event
                            currentState = PlayerState.Falling;
                            UpdateAnimations();
                            // Flip Sprite
                            spriteRenderer.flipX = !spriteRenderer.flipX;
                        }
                        else if (wallJumpTriggered)
                        {
                            OnStopWallSliding?.Invoke();        // Trigger Stop Wall Sliding Event
                            TriggerWallJump();
                            OnWallJump?.Invoke();               // Trigger Wall Jump Event
                            currentState = PlayerState.WallJumping;
                            UpdateAnimations();
                        }
                    }
                    Debug.Log("From WallBraking state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case PlayerState.Swinging:
                    if (jumpTriggered)
                    {
                        OnStopRopeSwinging?.Invoke();        // Trigger Stop Rope Swinging Event
                        TriggerJump();
                        OnHookRelease?.Invoke();            // Trigger Hook Release (Hook Jump) Event        

                        currentState = PlayerState.Jumping;
                        UpdateAnimations();

                        //Debug.Log("From Swinging state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                default:
                    // Default logic
                    break;
            }
        }
    }
    public void TriggerHurtingState()
    {
        isHurt = true;
        currentState = PlayerState.Hurting;
        UpdateAnimations();
    }
    public void DisableHurtingState()
    {
        isHurt = false;
        //ResetVelocity();
    }
    //////////////////////////////////////////////////
    #endregion

    #region Raycast
    void RaycastGrounded()
    {
        // Raycast Launching
        //isGrounded = Physics2D.Raycast(groundChecks[0].position, Vector2.down, rayLength, groundLayer);

        // Raycast Launching
        RaycastHit2D[] raycastsHit2D = new RaycastHit2D[groundChecks.Count()];
        isRayGroundDetected = false;
        for (int i = 0; i < groundChecks.Count(); i++)
        {
            raycastsHit2D[i] = Physics2D.Raycast(groundChecks[i].position, Vector2.down, rayLength, (int)groundLayer | (int)platformLayer);
            isRayGroundDetected |= raycastsHit2D[i];
        }

        // Raycast Debugging
        //foreach(Transform groundCheck in groundChecks)
        //{
        //    Debug.DrawRay(groundCheck.position, Vector2.down * rayLength, Color.red);
        //    // Draw 2 aditional lines to make easier the raycast visualization
        //    //Debug.DrawRay(groundCheck.position + (Vector3.right * 0.01f), Vector2.down * rayLength, Color.red);
        //    //Debug.DrawRay(groundCheck.position + (Vector3.left * 0.01f), Vector2.down * rayLength, Color.red);
        //}
        Debug.DrawRay(groundChecks[0].position, Vector2.down * rayLength, Color.red);
        Debug.DrawRay(groundChecks[1].position, Vector2.down * rayLength, Color.blue);
        Debug.DrawRay(groundChecks[2].position, Vector2.down * rayLength, Color.green);
    }
    void RaycastCeiling()
    {
        // Raycast Launching
        RaycastHit2D raycastCornerLeft;        
        RaycastHit2D raycastCornerRight;        

        //cornerDetected = false;        
        raycastCornerLeft = Physics2D.Raycast(cornerLeftCheck.position, Vector2.up, rayCornerLength, (int)groundLayer | (int)platformLayer);
        raycastCornerRight = Physics2D.Raycast(cornerRightCheck.position, Vector2.up, rayCornerLength, (int)groundLayer | (int)platformLayer);

        // Update the corner detection
        if (raycastCornerLeft && raycastCornerRight)       
            cornerDetected = CornerDetected.Ceiling;        
        else if (!raycastCornerLeft && raycastCornerRight)        
            cornerDetected = CornerDetected.CornerLeft;        
        else if (raycastCornerLeft && !raycastCornerRight)        
            cornerDetected = CornerDetected.CornerRight;        
        else        
            cornerDetected = CornerDetected.NoCeiling;

        // Raycast Debugging        
        //Debug.DrawRay(cornerLeftCheck.position, Vector2.up * rayCornerLength, Color.green);
        //Debug.DrawRay(cornerRightCheck.position, Vector2.up * rayCornerLength, Color.green);
    }
    void RaycastVertWalls()
    {
        // Raycast Launching
        RaycastHit2D raycastWallFwd;

        // Calculate RaycastWallDirection & rayWallOrigin
        rayWallDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        rayWallOrigin = spriteRenderer.flipX ? wallLeftCheck.position : wallRightCheck.position;

        raycastWallFwd = Physics2D.Raycast(rayWallOrigin, rayWallDir, rayWallLength, wallLayer);

        // Update the Wall detection
        isRayWallDetected = raycastWallFwd;

        // Raycast Debugging        
        //Debug.DrawRay(rayWallOrigin, rayWallDir * rayWallLength, Color.blue);        
    }
    #endregion

    #region Coyote Time
    private void UdpateCoyoteTimer()
    {
        // Coyote Timer update
        coyoteTimer += Time.fixedDeltaTime;

        // Reset Coyote Timer
        if (coyoteTimer >= maxCoyoteTime)
        {
            ResetCoyoteTimer();
        }                           
    }
    private void ResetCoyoteTimer()
    {
        isCoyoteTimerEnabled = false;
        coyoteTimer = 0f;
    }
    private void CheckCoyoteTimer()
    {        
        // Coyote Timer will be triggered when the player stop touching the ground
        if ((wasGrounded && !isGrounded) && currentState != PlayerState.Jumping && !isCoyoteTimerEnabled) 
        {
            isCoyoteTimerEnabled = true;
            coyoteTimer = 0f;
        }            
    }
    #endregion

    #region Input Player     
    public virtual void JumpActionInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            jumpPressed = true;

            // Enable the Jump Buffer Timer            
            SetJumpBufferTimer();
        }

        if(context.phase == InputActionPhase.Canceled)
        {
            jumpPressed = false;            
        }            
    }
    public virtual void HookActionInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            hookActionPressed = true;            

            // Enable the Hook timer
            if (isJumping && isHookUnlocked)
                SetHookThrownTimer();
        }        

        if(context.phase == InputActionPhase.Canceled)
        {
            hookActionPressed = false;            
        }            
    }
    public virtual void MoveActionInput(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>();
        lastDirection = direction;                      

        // Block the inputX & Sprite flipping update during certain frames
        // When triggered THE WallJumpDelayTimer --> inputX = 0;                
        
        //UpdateInputAndSprite(direction);
    }
    private void UpdateInputAndSprite(Vector2 direction)
    {
        //// Apply the deadZone to the Horizontal movement        
        //inputX = (Mathf.Abs(direction.x) > inputDirDeadZone && !isWallJumpDelayEnabled) ?
        //        direction.x :
        //        0f;        

        if (IsKeyboardActive())
        {
            inputX = Mathf.MoveTowards(inputX, 
                                    direction.x, 
                                    smoothKeyboardSpeed * Time.deltaTime);
            //Debug.Log("Dir = " + direction.x);
            Debug.Log("InputX = " + inputX);
        }
        else
        {
            // Apply the deadZone to the Horizontal movement        
            inputX = (Mathf.Abs(direction.x) > inputDirDeadZone && !isWallJumpDelayEnabled) ?
                    direction.x :
                    0f;
        }

            // Flip the player sprite & change the animations State
            FlipSprite(inputX, inputDirDeadZone);

        //Debug.Log($"Held DirX: {direction.x} | inputX: {inputX} | Delta: {smoothKeyboardSpeed * Time.deltaTime}");
    }
    private bool IsKeyboardActive()
    {        
        return (Keyboard.current != null && Keyboard.current.anyKey.isPressed);
    }
    #region Jumping Buffer    
    private void UdpateJumpBufferTimer()
    {
        // Jump Buffer Timer update
        jumpBufferTimer -= Time.fixedDeltaTime;

        // Reset Jump Buffer Timer
        if (jumpBufferTimer <= 0)
        {
            ResetJumpBufferTimer();
        }
    }
    protected void SetJumpBufferTimer()
    {        
        isJumpBufferEnabled = true;
        jumpBufferTimer = jumpBufferTime;
    }
    private void ResetJumpBufferTimer()
    {
        isJumpBufferEnabled = false;
        jumpBufferTimer = 0f;
    }
    private void CheckJumpTrigger()
    {        
        // If a possible Normal Jump is detected (Either through isGrounded or through CoyoteTime)
        if((isGrounded || isCoyoteTimerEnabled) && isJumpBufferEnabled)
        {            
            // Reset the Jumping Buffer Timer
            ResetJumpBufferTimer();
            // Reset the Jumping Timer
            ResetJumpTimer();
            // Reset the Coyote Timer
            ResetCoyoteTimer();
            //Set the Jumping Horizontal Speed in func. of the max Horiz Jump Distance and the Max Jump Horiz time
            jumpHorizSpeed = maxJumpHorizDist / 
                            maxJumpHorizTime;

            // Register a Jumping Trigger Request
            jumpTriggered = true;            
        }
        // Otherwise, if a Wall Jump is detected (wallFwdDetected by raycastWallFwd)
        else if ((!isGrounded && isWallDetected) && isJumpBufferEnabled)
        {
            // Reset the Jumping Buffer Timer             
            ResetJumpBufferTimer();
            // Reset the Jumping Timer
            // (Will be used to calculate the min/maxJumpingTimes on CalculateJumpSpeedMovement)  
            ResetJumpTimer();

            //Set the Wall Jumping Horizontal Speed in func. of the max Wall Horiz Jump Distance and the Max Wall Jump Horiz time            
            wallJumpHorizSpeed = wallJumpHorizDist /
                                wallJumpHorizTime;

            //Set also a percentage of the Normal Jumping Horiz Speed in order to apply it when the player take
            //the control again. 
            jumpHorizSpeed = wallJumpHorizDist2 /
                            maxJumpHorizTime;

            // Register a Wall Jumping Trigger Request
            wallJumpTriggered = true;
            
            Debug.Log("Entered in Wall-Jump Triggered on State " + currentState);
            Debug.Log("Wall Jump Horiz Speed = " + wallJumpHorizSpeed);
            Debug.Log("Jump Horiz Speed Speed = " + jumpHorizSpeed);
        }
        else if (isRopeSwinging && isJumpBufferEnabled)
        {
            // Reset the Jumping Buffer Timer
            ResetJumpBufferTimer();
            // Reset the Jumping Timer
            ResetJumpTimer();
            //Set the Jumping Horizontal Speed in func. of the max Horiz Jump Distance and the Max Jump Horiz time
            //jumpHorizSpeed = maxJumpHorizDist /
            //                maxJumpHorizTime;
            jumpHorizSpeed = wallJumpHorizDist2 /
                            maxJumpHorizTime;

            // Register a Jumping Trigger Request
            jumpTriggered = true;            
        }
    }
    private void UpdateJumpTimer()
    {
        jumpingTimer += Time.fixedDeltaTime;
    }
    private void ResetJumpTimer()
    {
        jumpingTimer = 0;
    }
    #endregion
    #region Hook
    private void UdpateHookThrownTimer()
    {
        // Jump Buffer Timer update
        hookThrownTimer -= Time.fixedDeltaTime;

        // Reset Jump Buffer Timer
        if (hookThrownTimer <= 0)
        {
            ResetHookThrownTimer();
        }
    }
    protected void SetHookThrownTimer()
    {        
        // Trigger the Grappling Hook (Show Line Renderer + Enable Distance Joint 2D)        
        OnHookThrown?.Invoke();

        hookThrownTimer = hookThrownMaxTime;
        isHookThrownEnabled = true;
    }
    private void ResetHookThrownTimer()
    {
        isHookThrownEnabled = false;
        hookThrownTimer = 0f;

        // Disable the Grappling Hook after elapsed a certain time
        if(!playerHook.IsHookAttached)
            OnHookRelease?.Invoke();        
    }
    #endregion
    #region Hook
    private void UdpateWallJumpDelayTimer()
    {
        // Jump Buffer Timer update
        wallJumpDelayTimer -= Time.fixedDeltaTime;

        // Reset Jump Buffer Timer
        if (wallJumpDelayTimer <= 0)
        {
            ResetWallJumpDelayTimer();
        }
    }
    protected void SetWallJumpDelayTimer()
    {
        // Set to 0 the inputX to assure no sprite turns
        inputX = 0f;

        wallJumpDelayTimer = wallJumpDelayMaxTime;
        isWallJumpDelayEnabled = true;
    }
    private void ResetWallJumpDelayTimer()
    {
        isWallJumpDelayEnabled = false;
        wallJumpDelayTimer = 0f;

        UpdateInputAndSprite(lastDirection);
    }
    #endregion
    #endregion

    #region RigidBody
    public void SetRbAsKinematics()
    {
        rb2D.bodyType = RigidbodyType2D.Kinematic;
    }
    public void SetRbAsDynamics()
    {        
        rb2D.bodyType = RigidbodyType2D.Dynamic;
    }
    #endregion

    #region Collider
    public void EnableCollider()
    {
        boxCollider2D.enabled = true;
    }
    public void DisableCollider()
    {
        boxCollider2D.enabled = false;
    }
    #endregion

    #region Jumping
    void TriggerJump()
    {        
        // Clear the jumpTriggered Flag & Reset the Coyote Timer (Avoid to enter on undesired States)
        jumpTriggered = false;        

        // Enable the isJumpTriggered for a certain time;
        isRecentlyJumping = true;
        //Invoke(nameof(DisableIsJumpTriggered),0.2f);
        StartCoroutine(nameof(DisableJumpTriggerFlag));

        // Fix Player's position due to corner detection
        if (cornerDetected == CornerDetected.CornerLeft)
            transform.position -= new Vector3(0.7f, 0f);
        else if (cornerDetected == CornerDetected.CornerRight)
            transform.position += new Vector3(0.7f, 0f);

        CalculateJumpTimes(jumpVertSpeed);        
    }
    IEnumerator DisableJumpTriggerFlag()
    {
        yield return new WaitForSeconds(0.2f);
        isRecentlyJumping = false;
    }
    //void DisableJumpTriggerFlag()
    //{
    //    isJumpTriggered = false;
    //}
    void TriggerWallJump()
    {        
        // Clear the wallJumpTriggered Flag (Avoid to enter on undesired States)
        wallJumpTriggered = false;

        // Enable the isWallJumpTriggered for a certain time;
        isRecentlyWallJumping = true;
        //Invoke(nameof(DisableWallJumpTriggerFlag), 0.2f);        
        StartCoroutine(nameof(DisableWallJumpTriggerFlag));

        //CalculateWallJumpTimes();                                                                   
        CalculateJumpTimes(wallJumpVertSpeed);

        // Reset Rb velocity to avoid inconsistencies        
        ResetVelocity();

        // Blocks all the changes on inputX & Sprite flipping during certain frames
        SetWallJumpDelayTimer();

        //wallSpeedVector = (-rayWallDir * Mathf.Cos(Mathf.Deg2Rad * 30) * wallJumpForce) +
        //                    (Vector2.up * Mathf.Sin(Mathf.Deg2Rad * 30) * wallJumpForce);
        
        wallJumpSpeedVector = (-rayWallDir * wallJumpHorizSpeed) +
                            (Vector2.up * wallJumpVertSpeed);

        //rb2D.AddForce(wallForce, ForceMode2D.Impulse);
        //rb2D.velocity = wallJumpSpeedVector;        

        // Flip Sprite
        spriteRenderer.flipX = !spriteRenderer.flipX;

        Debug.Log("Wall Jump Speed Vector = " + wallJumpSpeedVector);
    }
    IEnumerator DisableWallJumpTriggerFlag()
    {
        yield return new WaitForSeconds(0.2f);
        isRecentlyWallJumping = false;
    }
    //void DisableWallJumpTriggerFlag()
    //{
    //    isWallJumpTriggered = false;
    //}
    void CalculateJumpTimes(float jumpSpeed)
    {
        // Solve the MRUA equation--> h = v0*t - (1/2)g*(t^2);

        float discrimMinJumpTime = Mathf.Pow(jumpSpeed, 2) - 2 * Physics2D.gravity.y * minJumpVertDist;
        float discrimMaxJumpTime = Mathf.Pow(jumpSpeed, 2) - 2 * Physics2D.gravity.y * maxJumpVertDist;

        if (discrimMinJumpTime >= 0)
            minJumpingTime = (jumpSpeed - Mathf.Sqrt(discrimMinJumpTime)) / Physics2D.gravity.y;
        else
            // Jumping not posible, not enought initial speed
            Debug.LogError("The jumping is not posible with the initial speed and desired height");

        if (discrimMaxJumpTime >= 0)
            maxJumpingTime = (jumpSpeed - Mathf.Sqrt(discrimMaxJumpTime)) / Physics2D.gravity.y;
        else
            // Jumping not posible, not enought initial speed
            Debug.LogError("The jumping is not posible with the initial speed and desired height");

        // Max Jumping Time Correction
        maxJumpingTime += 0.03f;
    }
    //void CalculateWallJumpTimes()
    //{
    //    // Solve the MRUA equation--> h = v0*t - (1/2)g*(t^2);

    //    float discrimWallJumpTime = Mathf.Pow(wallJumpVertSpeed, 2) - 2 * Physics2D.gravity.y * wallJumpVertDist;        

    //    if (discrimWallJumpTime >= 0)
    //        wallJumpingVertTime = (wallJumpVertSpeed - Mathf.Sqrt(discrimWallJumpTime)) / Physics2D.gravity.y;
    //    else
    //        // Wall Jumping not posible, not enought initial speed
    //        Debug.LogError("The Wall jumping is not posible with the initial speed and desired height");        

    //    // Wall Jumping Time Correction
    //    //wallJumpingTime += 0.03f;
    //}
    #endregion

    #region Movement
    void UpdateHorizSpeed()
    {
        float filteredInputX = 0f;

        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
                rb2DDirVelX = inputX * speed;
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                rb2DDirVelX = inputX * jumpHorizSpeed;
                // Add a little push on the opposite dir. of the Wall if Wall Detected
                //if (isWallDetected)
                //    rb2DDirVelX += jumpWallPush * (-rayWallDir.x); 
                break;
            case PlayerState.WallJumping:                
                // X-velocity starts decreasing after a certain time
                if (jumpingTimer >= wallJumpHorizTime)
                {
                    rb2DDirVelX = 0;
                }
                else
                {
                    // Jumping force through velocity
                    rb2DDirVelX = wallJumpSpeedVector.x;
                    //rb2D.velocity = new Vector2(rb2D.velocity.x,rb2DJumpVelY);
                }
                break;
            case PlayerState.WallBraking:

                //rayWallDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

                // If rayWallDir = Vector2.right --> Wall on the right'side of the player
                // If rayWallDir = Vector2.left-- > Wall on the left'side of the player
                filteredInputX = (rayWallDir.x > 0) ? 
                                Mathf.Clamp(inputX, -1, 0) : Mathf.Clamp(inputX, 0, 1);                

                rb2DDirVelX = filteredInputX * jumpHorizSpeed;
                //rb2DDirVelX = 0;
                break;
            case PlayerState.Swinging:
                //// Define min and max angles
                //float minSwingAngle = 10f;
                //float maxSwingAngle = 170f;
                //// Define max Swing Speed
                //float maxSwingSpeed = 5;                

                // Calculate Swinging Factor in func. of the rope angle
                float ropeAngle = Mathf.Clamp(playerHook.CurrentRopeAngle, 0, 180);                
                float factorSwingSpeed = Mathf.Sin(ropeAngle * Mathf.Deg2Rad);
                factorSwingSpeed = Mathf.Clamp(factorSwingSpeed,0.2f,1f);

                factorSwingSpeed = Mathf.Pow(factorSwingSpeed,3);
                
                Vector2 vectorSwingForce = Vector2.right * (inputX * factorSwingSpeed) * swingHorizForce;

                // Opcional: chequear si estás cerca de los límites y bloquear la fuerza hacia fuera
                if ((ropeAngle <= minSwingAngle /*&& inputX < 0*/) || (ropeAngle >= maxSwingAngle /*&& inputX > 0*/))
                {
                    vectorSwingForce = Vector2.zero; // Sin fuerza para evitar pasar el límite
                }

                // Add extra friction in case the player is looking to the opposite movement direction
                int swingingArea = (-Mathf.Cos(ropeAngle * Mathf.Deg2Rad) > 0) ? 1 : -1;
                int playerDir = spriteRenderer.flipX ? -1 : 1;
                if (playerDir != swingingArea)
                    vectorSwingForce += Vector2.right * playerDir * swingHorizForce * opposeSwingForceFactor;

                rb2D.AddForce(vectorSwingForce);                    

                // Además, limitar la velocidad horizontal (clamp) para que no se pase
                float clampedVelX = Mathf.Clamp(rb2D.linearVelocityX, -maxSwingSpeed, maxSwingSpeed);
                rb2D.linearVelocity = new Vector2(clampedVelX, rb2D.linearVelocityY);

                break;
            case PlayerState.Hurting:
                if (thrustEnemyTimer >= thrustEnemyDuration * 0.3f)
                    rb2DDirVelX = enemyHitDir.x * enemyHitThrust;
                else                    
                    rb2DDirVelX *= 0.5f;
                break;
            default:
                break;
        }
    }
    void UpdateVerticalSpeed()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
            case PlayerState.Falling:            
                //rb2DJumpVelY = rb2D.linearVelocity.y;
                rb2DJumpVelY = Mathf.Clamp(rb2D.linearVelocity.y,-18f,5f);
                break;
            case PlayerState.WallBraking:
                //rb2DJumpVelY = rb2D.linearVelocity.y;
                rb2DJumpVelY = Mathf.Clamp(rb2D.linearVelocity.y, -10f, 0f);
                break;
            case PlayerState.Jumping:
            case PlayerState.WallJumping:
                // Jump button released and elapsed min Time
                if (jumpingTimer >= minJumpingTime)
                {
                    // Stop giving jump speed;            
                    if (!jumpPressed || jumpingTimer >= maxJumpingTime)
                    {
                        rb2DJumpVelY *= 0.5f;

                        //TEST
                        jumpingTimer = maxJumpingTime;       // --> FORCE TO MAX_TIME for avoiding to detect false jumps
                    }
                    else
                    {
                        if (currentState == PlayerState.Jumping)
                            rb2DJumpVelY = Vector2.up.y * jumpVertSpeed;
                        else if (currentState == PlayerState.WallJumping)
                            rb2DJumpVelY = Vector2.up.y * wallJumpVertSpeed;
                    }                        
                }
                else
                {                    
                    if (currentState == PlayerState.Jumping)
                        rb2DJumpVelY = Vector2.up.y * jumpVertSpeed;
                    else if (currentState == PlayerState.WallJumping)
                        rb2DJumpVelY = Vector2.up.y * wallJumpVertSpeed;
                }
                break;
            case PlayerState.Swinging:
                //rb2DJumpVelY = rb2D.linearVelocity.y;
                break;
            case PlayerState.Hurting:                
                if (thrustEnemyTimer >= thrustEnemyDuration * 0.3f)
                    rb2DJumpVelY = enemyHitDir.y * enemyHitThrust * 0.6f;                
                else
                    rb2DJumpVelY *= 0.5f;
                break;                
            default:
                break;
        }

        // Jumping force through Add Force
        //rb2D.AddForce(Vector2.up*jumpForce);               
    }    
    void UpdatePlayerSpeed()
    {
        targetVelocity = new Vector2(rb2DDirVelX, rb2DJumpVelY);

        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Running:
                // Smooth both axis on normal states
                //rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, targetVelocity, ref dampVelocity, smoothTime);
                rb2D.linearVelocity = Vector2.Lerp(rb2D.linearVelocity, targetVelocity, Time.fixedDeltaTime * lerpSpeed);
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                // Don't smooth the Y-axis while jumping
                //rb2D.velocity = new Vector2(
                //                Mathf.SmoothDamp(rb2D.velocity.x, targetVelocity.x, ref dampVelocity.x, smoothTime),
                //                targetVelocity.y);

                rb2D.linearVelocity = new Vector2(
                                Mathf.Lerp(rb2D.linearVelocity.x, targetVelocity.x, Time.fixedDeltaTime * lerpSpeed),
                                targetVelocity.y);
                //rb2D.velocity = targetVelocity;
                break;
            case PlayerState.WallBraking:
                rb2D.linearVelocity = targetVelocity;
                // Try the same as on Idle and Running States? (To check the feeling)
                //rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetVelocity, Time.fixedDeltaTime * lerpSpeed);
                break;
            case PlayerState.WallJumping:                      
                rb2D.linearVelocity = targetVelocity;
                break;
            case PlayerState.Swinging:
                //rb2D.linearVelocity = targetVelocity;

                //if (inputX == 0)
                //rb2D.linearVelocity = new Vector2(rb2DDirVelX, rb2D.linearVelocityY);

                //rb2D.linearVelocity = new Vector2(
                //                Mathf.Lerp(rb2D.linearVelocity.x, targetVelocity.x, Time.fixedDeltaTime * lerpSpeed),
                //                targetVelocity.y);

                break;
            case PlayerState.Hurting:
                rb2D.linearVelocity = targetVelocity;
                break;
            default:
                break;
        }
    }
    private void ChangeGravity()
    {
        //Gravity will be heavier when the player is falling down
        if (currentState == PlayerState.WallBraking)
        {            
            rb2D.gravityScale = 0.5f;
        }        
        else if (currentState == PlayerState.Falling)
        {
            if (isDead)
                rb2D.gravityScale = 5f;
            else
                rb2D.gravityScale = 2.5f;
        }
        else
        {
            if (isDead)
                rb2D.gravityScale = 2f;
            else
                rb2D.gravityScale = 1f;
        }        
    }
    public void ResetVelocity()
    {
        // Stops the player resetting its velocity
        rb2D.linearVelocity = Vector2.zero;
    }
    private void ResetPlayerSpeedBeforeSwinging()
    {        
        Vector2 currentVel = rb2D.linearVelocity;
        maxInitialSwingSpeed = 3f;
        currentVel.x = Mathf.Clamp(currentVel.x, -maxInitialSwingSpeed, maxInitialSwingSpeed);
        currentVel.y = Mathf.Clamp(currentVel.y, -maxInitialSwingSpeed, maxInitialSwingSpeed);
        rb2D.linearVelocity = currentVel;
        //rb2D.linearVelocity = Vector2.zero;
    }
    #endregion

    #region Attack
    //private void AttackEnemyOld(GameObject enemy)
    //{
    //    if(isGrounded)
    //        return;

    //    rb2D.AddForce(Vector2.up * jumpForce);
    //    enemy.GetComponent<Animator>().SetTrigger("Death");
    //    enemy.GetComponent<Rat>().PlayDeathFx();
    //    enemy.GetComponent<Rat>().DisablePlayerDetection();
    //    Destroy(enemy,0.5f);
    //}
    public void UpwardsEnemyImpulse()
    {        
        rb2D.AddForce(Vector2.up * jumpEnemyThrust);        
    }
    #endregion

    #region Damage
    private void UdpateThrustEnemyTimer()
    {
        // Jump Buffer Timer update
        thrustEnemyTimer -= Time.fixedDeltaTime;

        // Reset Jump Buffer Timer
        if (thrustEnemyTimer <= 0)
        {
            ResetThrustEnemyTimer();
        }
    }
    private void SetThrustEnemyTimer()
    {
        isThrustEnemyTimerEnabled = true;
        thrustEnemyTimer = thrustEnemyDuration;
    }
    private void ResetThrustEnemyTimer()
    {
        isThrustEnemyTimerEnabled = false;
        thrustEnemyTimer = 0f;
    }
    public void ReceiveDamage(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        // Trigger the Hurting State (and also the hurting anim.)
        TriggerHurtingState();
        // reset the player's velocity        
        ResetVelocity();
        // Set the Thrust Velocities & the Thrust Enemy Timer;
        SetBackwardsThrustVelocities(thrustEnemyDir, thrustEnemyForce);
        SetThrustEnemyTimer();

        // Backwards Thrust to the player (on opposite direction to the enemy's path)
        //StartCoroutine(TriggerBackwardsThrustCoroutine(thrustEnemyDir, thrustEnemyForce, ForceMode2D.Force);
        //BackwardsThrustEnemy(thrustEnemyDir, thrustEnemyForce, ForceMode2D.Force);
        // Disable the Player's Collider during the Hurt Animation & reset the Hurt animation after 1s.
        StartCoroutine(nameof(ExitHurtingStateAfterDelay));              
    }
    private IEnumerator ExitHurtingStateAfterDelay()
    {
        //DisableDamage();

        //yield return new WaitUntil(() => (playerVFX.FadingTimer >= playerVFX.FadingTotalDuration));
        yield return new WaitWhile(()=>isThrustEnemyTimerEnabled);

        // Finish the Hurting State
        DisableHurtingState();

        // Re-enable the Player's Damage
        //EnableDamage();
    }
    private void DisableDamage()
    {
        // Set the Player's Rb as Kinematics
        SetRbAsKinematics();
        // Disable the Player's Collider
        DisableCollider();
    }
    private void EnableDamage()
    {
        // Reenable the Player's Collider
        EnableCollider();
        // Set the Player's Rb as Dynamics again
        SetRbAsDynamics();
    }
    private void SetBackwardsThrustVelocities(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
         enemyHitDir = thrustEnemyDir;
         enemyHitThrust = thrustEnemyForce;
    }
    private IEnumerator TriggerBackwardsThrustCoroutine(Vector2 thrustDir, float thrustForce, ForceMode2D forceMode2D)
    {
        yield return new WaitUntil(() => Time.timeScale == 1f);

        // Backwards Thrust to the player (on opposite direction to the enemy's path)
        BackwardsThrustEnemy(thrustDir, thrustForce, forceMode2D);
        //BackwardsThrustVelEnemy(thrustDir, thrustForce);
    }    
    private void BackwardsThrustEnemy(Vector2 thrustDir, float thrustForce, ForceMode2D forceMode2D)
    {        
        if (forceMode2D == ForceMode2D.Force)
            rb2D.AddForce(new Vector2(thrustDir.x * thrustForce,
                                    thrustDir.y * thrustForce * 0.3f), ForceMode2D.Force);
        else
            rb2D.AddForce(new Vector2(thrustDir.x * thrustForce, 
                                    thrustDir.y * thrustForce * 0.3f), ForceMode2D.Impulse);
    }
    private void BackwardsThrustVelEnemy(Vector2 thrustDir, float thrustForce)
    {        
        rb2D.linearVelocity = new Vector2(thrustDir.x * thrustForce*3,thrustDir.y * thrustForce);
    }
    #endregion
    #region Death
    private void Death()
    {
        // Set the Player isDead Flag
        isDead = true;
        // Disable the Vitamini's Circle Collider & Apply an upwards impulse
        DisableCollider();
        rb2D.AddForce(Vector2.up * forceJumpDeath);
        // Set the New Local Scale
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(2.5f, 2.5f, 2.5f));
    }
    #endregion

    #region Sprite & Animations
    // Flip the Player sprite in function of its movement
    protected void FlipSprite(float horizontal, float deadZone)
    {
        // Evitar que el sprite se gire si acabamos de hacer wall jump
        if (/*isRecentlyWallJumping ||*/ currentState == PlayerState.WallBraking)
            return;

        //if (horizontal != 0)
        //    spriteRenderer.flipX = horizontal < 0;

        if (Mathf.Abs(horizontal) > deadZone)
            spriteRenderer.flipX = horizontal < 0;
    }
    //void UpdateBoxCollider()
    //{
    //    if(spriteRenderer.flipX != lastFlipState)
    //    {
    //        if (spriteRenderer.flipX)
    //            boxCollider2D.offset = new Vector2(0.16f, boxCollider2D.offset.y);
    //        else
    //            boxCollider2D.offset = new Vector2(-0.16f, boxCollider2D.offset.y);
    //    }
    //}
    private void AnimatingRunning(float horizontal)
    {
        //animator.SetBool("IsRunning", horizontal != 0);
        animator.SetBool("IsRunning", currentState == PlayerState.Running);
    }
    private void AnimatingJumping()
    {
        //animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsJumping", currentState == PlayerState.Jumping ||
                                    currentState == PlayerState.WallJumping);
    }
    private void AnimatingFalling()
    {
        animator.SetBool("IsFalling", currentState == PlayerState.Falling);
    }
    private void ClearAnimationFlags()
    {
        animator.ResetTrigger("IsIdle");
        animator.ResetTrigger("IsRunning");
        animator.ResetTrigger("IsJumping");
        animator.ResetTrigger("IsFalling");
        animator.ResetTrigger("IsWallSliding");
        animator.ResetTrigger("IsSwinging");
        animator.ResetTrigger("Hurt");
    }
    private void UpdateAnimations_(string triggerParamName)
    {
        animator.SetTrigger(triggerParamName);
    }
    private void UpdateAnimationSpeed()
    {
        animator.SetFloat("Speed", Math.Abs(rb2D.linearVelocityX));
    }
    private void UpdateAnimations()
    {
        ClearAnimationFlags();

        switch (currentState)
        {
            case PlayerState.Idle:
                animator.SetTrigger("IsIdle");
                break;
            case PlayerState.Running:
                animator.SetTrigger("IsRunning");
                break;
            case PlayerState.Jumping:
            case PlayerState.WallJumping:
                animator.SetTrigger("IsJumping");
                break;
            case PlayerState.Falling:
                animator.SetTrigger("IsFalling");
                break;
            case PlayerState.WallBraking:
                animator.SetTrigger("IsWallSliding");
                break;
            case PlayerState.Swinging:
                animator.SetTrigger("IsSwinging");
                break;
            case PlayerState.Hurting:
                animator.SetTrigger("Hurt");                              
                break;
            default:
                break;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState == PlayerState.Falling && stateInfo.IsName("IsIdle") &&
            isWallDetected)
            Debug.Log("Current State is " + currentState + 
                " ;; IsFalling: " + animator.GetBool("IsFalling"));
    }

    #endregion

    #region CollectibleItems
    public void UnlockPowerUp(ItemTypeEnum.ItemType item)
    {
        if (item == ItemTypeEnum.ItemType.ClimbingBoots)
        {
            isWallJumpUnlocked = true;
            Debug.Log("Climbing Boots Unlocked!");
        }            
        else if (item == ItemTypeEnum.ItemType.Hook)
        {
            isHookUnlocked = true;
            Debug.Log("Grappling-Hook Unlocked!");
        }            
        else
            Debug.LogError("The opened chest contains neither the boots nor the hook");
    }
    public void UnlockKey()
    {
        isKeyUnlocked = true;
        Debug.Log("Golden Key Unlocked!");
    }
    public void IncreaseGems()
    {
        // Increase Gems counter
        NumGems++;
        // Update Gems counter UI Text
        textGemsUI.text = NumGems.ToString();
    }
    public void IncreaseLifes()
    {
        // Increase Lifes counter
        NumLifes++;
        // Update Lifes counter UI Text
        textLifesUI.text = NumLifes.ToString();
    }
    #endregion

    #region Scene Management
    private void LoadSceneAfterDelay()
    {
        Invoke(nameof(LoadScene),3f);
    }
    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level01");
    }
    #endregion    
    // Old Input System
    //void InputPlayer()
    //{
    //    horizontal = Input.GetAxis("Horizontal");
    //}    
}
