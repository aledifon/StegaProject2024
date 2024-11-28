using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Define Character States
    private enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Hurting
    }
    [Header("Character State")]
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    [Header("Movement")]
    [SerializeField] private float speed;
    //[SerializeField] private float forceMovement;   // Player's movement force (on Add Force Movement type through rb2D)
    [SerializeField] private float smoothTime;
    [SerializeField, Range(0.7f,1f)] private float playerSpeedAirCtrl;
    [SerializeField, Range(0f, 100000f)] private float frictionValue;

    [Header("Raycast")]
    [SerializeField] private float rayLength;       //ray Length    
    [SerializeField] private LayerMask groundLayer; //Layer where the ground will be contained
    [SerializeField] private Transform groundCheck; //origin raycast point

    [Header("Jump")]
    [SerializeField] private float jumpForce;

    // Movement vars.
    private float horizontal;
    private Vector2 inputPlayerVelocity;    // Velocity given by the player's input
    private Vector2 newRbVelocity;          // Desired target player Speed(Velocity Movement type through rb2D)
    public Vector2 dampVelocity;            // Player's current speed storage (Velocity Movement type through rb2D)

    // Movement Flags
    private bool isGrounded,                // Indicates if we are or not touching the ground
                canPlayerJump;              // Tells me if I can jump or not
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

    void Start()
    {
        anim = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();        
    }

    private void Update()
    {
        UpdateState();      // Update the player state

        InputPlayer();      // Gets player input movement        
        IsGrounded();       // Detect the ground and the Normal terrain vector
        canJump();          // Check if the player request a jump
         
        Flip();             // Flip the player sprite
        HandleAnimation();  // Animation Handling
    }

    private void FixedUpdate()
    {
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
                // Si el jugador saltó, aplica la componente normal
                if (canPlayerJump)
                {
                    // Reset the jump flag                                                        
                    canPlayerJump = false;
                    // Calculate the new jump component for the velocity
                    Vector2 jumpVelocity = normalVector * jumpForce;

                    // Combines jump and player velocity to obtain the new velocity vector
                    //newRbVelocity += jumpVelocity;

                    // Apply Jumping impulse
                    rb2D.AddForce(jumpVelocity, ForceMode2D.Impulse);
                }

                // Frenado al dejar de moverse
                if (Mathf.Abs(horizontal) < Mathf.Epsilon)
                    newRbVelocity.x = Mathf.Lerp(newRbVelocity.x, 0, Time.deltaTime * frictionValue); // Ajusta el factor de 5f para mayor o menor fricción            
                else
                    newRbVelocity += inputPlayerVelocity;
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:                
                inputPlayerVelocity.x *= playerSpeedAirCtrl;    // Gets a % of the total input player's speed
                // Horiz. speed movement in the air
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
    }
    void canJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            canPlayerJump = true;
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
                Debug.DrawRay(hit.point, normalVector * 2f, Color.green, 1f);
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
        if (horizontal < 0) spriteRenderer.flipX = true;
        else if (horizontal > 0) spriteRenderer.flipX = false;
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
}
