using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float smoothTime;

    [Header("Raycast")]
    public float rayLength;//ray Length    
    public LayerMask groundLayer;//Layer where the ground will be contained
    public Transform groundCheck;//origin raycast point

    [Header("Jump")]
    public float jumpForce;

    // Movement vars.
    private float horizontal;
    private bool canMove;
    private Vector2 moveDirection;  // Player's movement direction (on Mov. Pos or Add Force Movement type through rb2D)
    private float forceMovement;    // Player's movement force (on Add Force Movement type through rb2D)
    private Vector2 targetVelocity; // Desired target player Speed (Velocity Movement type through rb2D)
    private Vector2 dampVelocity;   // Player's current speed storage (Velocity Movement type through rb2D)

    // Movement Flags
    private bool isGrounded,    //var which indicates if we are or not touching the ground
                canPlayerJump;  //Tells me if I can jump or not

    // Raycast & Normal vector vars.    
    private Vector2 raycastOffset = new Vector3(0f, 0.1f);  // Raycast origin offset to assure it will touch the floor 
    private Vector2 normalVector = Vector2.up;              // Terrain normal vector in the current frame
    //private Vector2 previousNormal = Vector2.up;            // Terrain normal vector in the previous frame

    //Slope vars.
    private bool isOnSlope;      //Indicates if we are on flat floor or a slope
    //private bool isOnSlopeUp;//var which indicates if we are on flat floor or a slope
    //private bool isOnSlopeDown;//var which indicates if we are on flat floor or a slope
    private Vector2 slopeNormalPerp;//Perpendicular vector to the Normal one with the Floor
    private float slopeDownAngle;//Angle between the Normal vector and the +Y direction.
    private float slopeDownAngleOld;//Former value of slopeDownAngle

    // GO Components
    private Animator anim;
    //private CircleCollider2D collider;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;    

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        canMove = true;
    }
    
    void Update()
    {
        //If Hurting Animation is running then won't execute the Update method
        if (anim.GetBool("Hurt")) return;

        InputPlayer();
        Flip();
        Animating();
        IsGrounded();       // Detect the ground and the Normal terrain vector
        //IsOnSlope();
        canJump();          
        //Attack();
        TargetVelocity();        
    }
    void FixedUpdate()
    {        
        //We call the player's movement through RigidBody only once per frame)
        if (canPlayerJump) 
        {
            canPlayerJump = false;
            Jump();                     // Jumping handling
        }
        else
        {            
            Move();                     // Player's movement handling (through Velocity Movement Mode)
        }
        Debug.Log("RigiBody.Velocity AFTER JUMP = (" + rb2D.velocity.x + " ," + rb2D.velocity.y + " )");

        //ChangeGravity();
    }

    // Player's movement
    void InputPlayer()
    {
        horizontal = Input.GetAxis("Horizontal");
    }
    void Move()
    {
        // Types of RigidBody Movement
        //Velocity: Ideal para plataformas 2D o juegos de movimiento constante.
        //Ejemplo: Side-scrollers, juegos top-down con controles precisos.

        rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, targetVelocity, ref dampVelocity, smoothTime);        
        //Debug.Log("rb2D.velocity = (" + rb2D.velocity.x + " ," + rb2D.velocity.y + " )");

        //MovePosition: Para movimientos precisos sin necesidad de físicas dinámicas.
        //Ejemplo: Juegos tipo puzzle o grid-based.
        //Vector2 newPosition = rb2D.position + moveDirection * speed * Time.fixedDeltaTime;
        //rb2D.MovePosition(newPosition);

        //AddForce:Ideal para juegos basados en física o con movimientos más naturales y fluidos.
        //        Ejemplo: Juegos con gravedad personalizada o empujones.
        //rb2D.AddForce(moveDirection * forceMovement);
    }    
    void TargetVelocity()
    {
        // Velocity Movement
        targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y);

        // MovePosition Movement or AddForce Movement
        //moveDirection = new Vector2(horizontal,0);

        //if (!isOnSlope)
        //    targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y);
        //else
        //    targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y*10);

    }
    void CanMoveToTrue()
    {
        canMove = true;
    }
    //public void ResetVelocity()
    //{
    //    targetVelocity = Vector2.zero;
    //}
    //////////////////////////////////////////////////

    // Player's animation
    void Animating()
    {
        //Running animation handling
        if (horizontal != 0) anim.SetBool("IsRunning", true);
        else anim.SetBool("IsRunning", false);
        //Jumping Animation handling (IsJumping param. will take the NOT value of isGrounded)
        anim.SetBool("IsJumping", !isGrounded);
    }
    void Flip()
    {
        if (horizontal < 0) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }
    //////////////////////////////////////////////////

    // Jumping methods
    void Jump()
    {        
        if (canMove && (normalVector != Vector2.zero))
        {
            // Reset the Rigidbody speeds before jumping
            //PrepareForJump();

            // 1. Calculates the force in the normal terrain direction
            Vector2 jumpDirection = normalVector * jumpForce; // Normalize the normal vector                        
            // 2. Adds a little forward force component 
            //Vector2 forwardImpulse = new Vector2(horizontal * (jumpForce / 2f), 0) ; // Reduced in order to don't be dominant in the jumping                                  
            //Calculates the resultant vector
            //Vector3 forceApplied = jumpDirection + forwardImpulse;
            Vector3 forceApplied = jumpDirection;

            //Debugging forces
            //Debug.Log("Normal dir. = (" + normalVector.x + " ," + normalVector.y + " )");
            ////Debug.Log("Fwd dir. = (" + forwardImpulse.x + " ," + forwardImpulse.y + " )");
            //Debug.Log("Force applied = (" + forceApplied.x + " ," + forceApplied.y + " )");
            ////Debugging RigidBody
            //Debug.Log("RigiBody.Velocity BEFORE JUMP = (" + rb2D.velocity.x + " ," + rb2D.velocity.y + " )");

            // Apply Jumping impulse
            rb2D.AddForce(forceApplied,ForceMode2D.Impulse);

            // Enable again the gravity after the jump impulse
            //rb2D.gravityScale = 1;
            //StartCoroutine(EnableGravityAfterDelay());
        }                    
    }
    void canJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) 
            canPlayerJump = true;
    }
    void PrepareForJump()
    {
        rb2D.velocity = new Vector2(rb2D.velocity.x, 0);    // Reset the speed on the y axis
        rb2D.angularVelocity = 0;                           // Reset the rotation speed
        rb2D.gravityScale = 0;                            // Optional: Disable temporary the gravity

        //Disable the Collider
        //collider.enabled = false;
    }
    public void ChangeGravity()
    {
        //If player is falling & is still alive then we'll increase the gravity value
        if (rb2D.velocity.y < 0 && playerHealth.currentHealth > 0) rb2D.gravityScale = 1.5f;
        //If player is falling & is still alive then we'll increase the gravity value
        else if (rb2D.velocity.y < 0 && playerHealth.currentHealth <= 0) rb2D.gravityScale = 3;
        //If player is rising then we'll keep gravity scale to def.
        else rb2D.gravityScale = 1;
    }
    IEnumerator EnableGravityAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // Espera un pequeño intervalo
        rb2D.gravityScale = 1;     // ReEnable the gravity

        //collider.enabled = true;    // ReEnable the player's collider
    }
    //////////////////////////////////////////////////

    // Raycast, grounding & Normal vector detection
    void IsGrounded()
    {
        //I launch a selective raycast (only detects the objects in the Ground Layer). 
        //It has rayLength as length, groundCheck as origin, down-Y direction.
        //The raycast return true if it is touching an object of the ground Layer
        //isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);
        //Debug.DrawRay(groundCheck.position, Vector2.down * rayLength, Color.red);

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
    void IsOnSlope()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);
        if (hit)
        {
            //Get the perpendicular vector to the normal (Takes the per. vector counting in clockwise dir.)
            slopeNormalPerp = Vector2.Perpendicular(hit.normal);
            //Get the angle between the Normal vector & the +Y vector)
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            //If the angle has changed we consider that we're in a Slope. Otherwise, we are in flat floor
            isOnSlope = (slopeDownAngle>5);
            
            //Update slope Down Angle value
            //slopeDownAngleOld = slopeDownAngle;

            //Debugging Normal & Perp. to Normal
            Debug.DrawRay(hit.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
    }
    //////////////////////////////////////////////////    
    
    // Collisions Detections
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Set the player as child of the platform
        if (collision.collider.CompareTag("Platform"))              
            transform.SetParent(collision.transform);
        
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        //Unset the player as child of the platform
        if (collision.collider.CompareTag("Platform"))
            transform.SetParent(null);
    }
    //////////////////////////////////////////////////

    //void Attack()
    //{
    //    //If I press the mouse button and the player is not moving
    //    if (Input.GetMouseButtonDown(0) && h == 0)
    //    {
    //        canMove = false;
    //        anim.SetTrigger("Attack");
    //    }
    //}
}
