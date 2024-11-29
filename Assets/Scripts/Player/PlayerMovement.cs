using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Define Character Movement Sense
    //private enum MovementState
    //{
    //    Stopped,
    //    Negative,
    //    Positive
    //}
    // Define Character States
    private enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Hurting
    }
    private enum JumpingState
    {
        WaitingForJumpRequest,
        CalculatingJumpForce,
        TriggeringJumpRequest,
        WaitingForJumpState,
        Jumping,
        DoubleJumping
    }
    [Header("Character State")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;
    [SerializeField] private JumpingState currentJumpState = JumpingState.WaitingForJumpRequest;

    [Header("Movement")]
    //[SerializeField] private MovementState currentMovState = MovementState.Stopped;
    [SerializeField] private bool isBraking;
    [SerializeField] private float speed;
    [SerializeField] private float playerMaxHorizSpeed;
    //[SerializeField] private float forceMovement;   // Player's movement force (on Add Force Movement type through rb2D)
    [SerializeField] private float smoothTime;
    [SerializeField, Range(0.7f,1f)] private float playerSpeedAirCtrl;
    [SerializeField, Range(0f, 100000f)] private float noMoveFriction;
    [SerializeField, Range(0f, 100000f)] private float changeMoveSenseFriction;

    [Header("Raycast")]
    [SerializeField] private float rayLength;       //ray Length    
    [SerializeField] private LayerMask groundLayer; //Layer where the ground will be contained
    [SerializeField] private Transform groundCheck; //origin raycast point

    [Header("Jump")]
    [SerializeField] private float jumpForce;       // Jumping applied Force
    [SerializeField] private float minJumpForce;    // Jumping Max Force (Equivalent to Jumping Max. Height)
    [SerializeField] private float maxJumpForce;    // Jumping Min Force (Equivalent to Jumping Min Height)
    [SerializeField] private float timeMaxJump;     // Jumping pressed button Max Time allowed

    // Movement vars.
    private float horizontal;
    private Vector2 inputPlayerVelocity;    // Velocity given by the player's input
    private Vector2 newRbVelocity;          // Desired target player Speed(Velocity Movement type through rb2D)
    public Vector2 dampVelocity;            // Player's current speed storage (Velocity Movement type through rb2D)    

    // Jumping vars
    private float timeJump;                 // Jumping Pressed button Timer    

    // Movement Flags
    private bool isGrounded,                // Indicates if we are or not touching the ground
                canPlayerJump,              // Tells me if I can jump or not
                canPlayerJump2;             // Double-Jump 
    // Animation Flags
    private bool isHurting;

    // Raycast & Normal vector vars.    
    private Vector2 raycastOffset = new Vector2(0f, 0.1f);  // Raycast origin offset to assure it will touch the floor 
    private Vector2 normalVector = Vector2.up;              // Terrain normal vector in the current frame    

    // GO Components
    private Animator anim;
    //private CircleCollider2D collider;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    private AudioSource audioJump;

    private void Awake()
    {
        // Configure globally the invariant culture
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioJump = GetComponent<AudioSource>();

        minJumpForce = 6f;
        maxJumpForce = 13f;
        timeMaxJump = 0.3f;
        //previousMovState = currentMovState;
    }

    private void Update()
    {
        UpdateState();      // Update the player state

        InputPlayer();          // Gets player input movement                
        IsGrounded();           // Detect the ground and the Normal terrain vector
        canJump();              // Check if the player request a jump
         
        Flip();                 // Flip the player sprite
        HandleAnimation();      // Animation Handling

        CheckAudioIsPlaying();
    }

    private void FixedUpdate()
    {
        CheckMovementSense();   // Detect player movement sense changes
        UdpateMovement();
    }

    // Player State
    private void UpdateState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                if (isGrounded && horizontal != 0)
                    currentState = PlayerState.Running;
                else if (!isGrounded && rb2D.velocity.y > 0)                
                    currentState = PlayerState.Jumping;
                else if (!isGrounded && rb2D.velocity.y < 0)
                    currentState = PlayerState.Falling;
                break;
            case PlayerState.Running:
                if (isGrounded && horizontal == 0)
                    currentState = PlayerState.Idle;
                else if(!isGrounded && rb2D.velocity.y > 0)
                    currentState = PlayerState.Jumping;
                else if (!isGrounded && rb2D.velocity.y < 0)
                    currentState = PlayerState.Falling;
                break;
            case PlayerState.Jumping:
                //if (isGrounded && horizontal == 0)
                //    currentState = PlayerState.Idle;
                //else if (isGrounded && horizontal != 0)
                //    currentState = PlayerState.Running;
                if (!isGrounded && rb2D.velocity.y < 0)
                    currentState = PlayerState.Falling;
                break;
            case PlayerState.Falling:
                if (isGrounded && horizontal == 0)
                    currentState = PlayerState.Idle;
                else if (isGrounded && horizontal != 0)
                    currentState = PlayerState.Running;
                break;
            case PlayerState.Hurting:
                // Hurting logic
                if (!isHurting)
                    currentState = PlayerState.Idle;
                break;
            default:
                // Default logic
                break;        
        }

        // From any State
        if (isHurting && currentState != PlayerState.Hurting)
            currentState = PlayerState.Hurting;
    }
    //////////////////////////////////////////////////

    // Player movement
    void InputPlayer()
    {
        horizontal = Input.GetAxis("Horizontal");          
    }
    void UdpateMovement()
    {
        // Update the Input player and RigidBody velocities
        newRbVelocity = rb2D.velocity;
        inputPlayerVelocity = new Vector2(horizontal * speed, 0);

        switch (currentState)
        {
            case PlayerState.Idle:                
            case PlayerState.Running:
                // Si el jugador saltó (and Max Jump Time elapsed), aplica la componente normal
                if (canPlayerJump)
                {                    
                    Debug.Log("JumpForce = " + jumpForce);  // Debugging applied force

                    // Reset the jump flag                                                        
                    canPlayerJump = false;
                    // Calculate the new jump component for the velocity
                    Vector2 jumpVelocity = normalVector * jumpForce;

                    // Combines jump and player velocity to obtain the new velocity vector
                    //newRbVelocity += jumpVelocity;

                    // Apply Jumping impulse
                    rb2D.AddForce(jumpVelocity, ForceMode2D.Impulse);

                    // Play audio jump effect
                    audioJump.Play();

                    //newRbVelocity += jumpVelocity;
                    //smoothTime = 0.05f;
                    //Invoke("ResetSmoothTime",0.5f);                    
                }                

                // Braking when there is no input player
                if (Mathf.Abs(horizontal) < Mathf.Epsilon)
                    newRbVelocity.x = Mathf.Lerp(newRbVelocity.x, 0, Time.deltaTime * noMoveFriction); // Ajusta el factor de frictionValue para mayor o menor fricción            
                // Braking when there is a movement sense change
                // Applied as long as Rb speed > 0 and player reaches 60% of MaxSpeed
                else if (isBraking && Mathf.Abs(rb2D.velocity.x)>Mathf.Epsilon)                
                    newRbVelocity.x = Mathf.Lerp(newRbVelocity.x, 0, Time.deltaTime * changeMoveSenseFriction); // Ajusta el factor de changeMoveSenseFriction para mayor o menor fricción                            
                // Speed Max. Horizontal Limitation (New Rb Speed not update when player reaches max speed)
                else if(Mathf.Abs(rb2D.velocity.x)<playerMaxHorizSpeed)                    
                    newRbVelocity += inputPlayerVelocity;
                break;            
            case PlayerState.Jumping:
            case PlayerState.Falling:
                if (canPlayerJump2)
                {
                    // Reset the jump flag                                                        
                    canPlayerJump2 = false;
                    // Calculate the new jump component for the velocity
                    Vector2 jumpVelocity = Vector2.up * maxJumpForce*0.5f;      // 50% of the max jump force value

                    // Apply Jumping impulse
                    rb2D.AddForce(jumpVelocity, ForceMode2D.Impulse);

                    // Play audio jump effect
                    audioJump.Play();
                }
                inputPlayerVelocity.x *= playerSpeedAirCtrl;    // Gets a % of the total input player's speed
                // Horiz. speed movement in the air
                //if (Mathf.Abs(rb2D.velocity.x) < (playerMaxHorizSpeed*playerSpeedAirCtrl))
                    newRbVelocity += inputPlayerVelocity;
                break;
            case PlayerState.Hurting:
                newRbVelocity = Vector2.zero;                   // Stops the player                
                break;
            default:
                // Default logic
                break;
        }

        // Applies the correspondent new velocity        
        rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, newRbVelocity, ref dampVelocity, smoothTime);
        Debug.Log("Rb.Velocity = (" + rb2D.velocity.x + " ," + rb2D.velocity.y + " )");
    }
    void CheckMovementSense()
    {
        // Change in the sense of movement happened and the player reached a 60% of Max. Speed
        if ((rb2D.velocity.x > 0 && horizontal < 0 || rb2D.velocity.x < 0 && horizontal > 0) &&
            Mathf.Abs(rb2D.velocity.x) > playerMaxHorizSpeed*0.6)
        {
            isBraking = true;
        }
        // Rigidbody and input player movement are aligned in the same sense.
        else if (rb2D.velocity.x > 0 && horizontal > 0 || rb2D.velocity.x < 0 && horizontal < 0)
        {
            isBraking = false;
        }            
    }
    void canJump()
    {
        //if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        //{
        //    canPlayerJump = true;
        //}        

        switch (currentJumpState)
        {            
            case JumpingState.WaitingForJumpRequest:
                // Trigger jumping timer when pressed Jumping buton and player is on the ground
                if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                {                                    
                    timeJump = 0;                   // Resets jumping timer
                    jumpForce = minJumpForce;       // Set jump force to min value.

                    //Invoke("EnablePlayerJump",timeMaxJump);

                    currentJumpState = JumpingState.CalculatingJumpForce;
                }
                break;
            case JumpingState.CalculatingJumpForce:
                // Update the Jumping force every frame as long as the 'jump' button is pressed
                if (Input.GetKey(KeyCode.Space) && (timeJump < timeMaxJump))
                {                                        
                    timeJump += Time.deltaTime;
                    // In case we go over the max Time value
                    if (timeJump > timeMaxJump)
                        timeJump = timeMaxJump;                                       
                }
                else                
                    currentJumpState = JumpingState.TriggeringJumpRequest;                
                break;            
            case JumpingState.TriggeringJumpRequest:
                // Calculate the jump force in func. of the time the jump button was pressed
                jumpForce = minJumpForce + (timeJump / timeMaxJump) * (maxJumpForce - minJumpForce);
                canPlayerJump = true;

                currentJumpState = JumpingState.WaitingForJumpState;
                break;
            case JumpingState.WaitingForJumpState:
                // Goes to jumping state when the movement player State will be also 'jumping'
                if (currentState == PlayerState.Jumping)
                    currentJumpState = JumpingState.Jumping;
                break;
            case JumpingState.Jumping:
                // If press again the jump button then a 2nd jump will be triggered
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    canPlayerJump2 = true;
                    currentJumpState = JumpingState.DoubleJumping;
                }
                // Otherwise once the player reaches the ground again then the jumping timer is reset
                // and comes back to the initial state
                else if (isGrounded)
                {
                    timeJump = 0;                   
                    currentJumpState = JumpingState.WaitingForJumpRequest;
                }                                        
                break;
            case JumpingState.DoubleJumping:
                // Once the player reaches the ground again then the jumping timer is reset
                // and come back to the initial state
                if (isGrounded)
                {
                    timeJump = 0;
                    currentJumpState = JumpingState.WaitingForJumpRequest;
                }
                break;
            default:
                break;
        }
    }
    void ResetSmoothTime()
    {
        smoothTime = 0.35f; 
    }
    //////////////////////////////////////////////////

    // Raycast, grounding & Normal vector detection
    void IsGrounded()
    {        
        // Initial pos. of the Raycast + offset
        Vector2 rayOrigin = (Vector2)groundCheck.position + raycastOffset; // Groundcheck position + offset
        // Raycast direction (downwards)
        Vector2 rayDirection = Vector2.down;

        // Raycast Launch
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, groundLayer);
        isGrounded = hit;
        // Raycast debugging
        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red);

        // In case of Raycast detection
        if (hit.collider != null)
        {
            // Gets Normal vector of the terrain
            normalVector = hit.normal;
            // Debugging the Normal vector of the terrain
            if (normalVector != Vector2.zero)
            {
                //Debug.DrawRay(hit.point, normalVector * 2f, Color.green, 1f);
            }
            else
            {
                Debug.LogWarning("La normal del terreno no es válida.");
            }
        }        
    }    
    //////////////////////////////////////////////////

    // Player animation    
    void Flip()
    {
        if (horizontal < 0 && rb2D.velocity.x < 0) 
            spriteRenderer.flipX = true;
        else if (horizontal > 0 && rb2D.velocity.x > 0) 
            spriteRenderer.flipX = false;
    }
    public void HurtToTrue()
    {
        isHurting = true;        
    }
    public void HurtToFalse()
    {
        isHurting = false;
    }
    private void HandleAnimation()
    {
        anim.SetBool("IsRunning",
           currentState == PlayerState.Running);

        anim.SetBool("IsJumping",
           currentState == PlayerState.Jumping ||
           currentState == PlayerState.Falling);        

        anim.SetBool("Hurt",
           currentState == PlayerState.Hurting);

        ////Running animation handling
        //if (horizontal != 0) anim.SetBool("IsRunning", true);
        //else anim.SetBool("IsRunning", false);
        ////Jumping Animation handling (IsJumping param. will take the NOT value of isGrounded)
        //anim.SetBool("IsJumping", !isGrounded);
    }
    //////////////////////////////////////////////////
    
    // Audio effects
    void CheckAudioIsPlaying()
    {
        // Check if audio finished playing
        if (!audioJump.isPlaying)        
            audioJump.Stop();        
    }
}
