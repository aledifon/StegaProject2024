using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float smoothTime;
    bool canMove;
    float horizontal;

    [Header("Raycast")]
    public Transform groundCheck;//origin raycast point
    public LayerMask groundLayer;//Layer where the ground will be contained
    public float rayLength;//ray Length
    public bool isGrounded;//var which indicates if we are or not touching the ground
    public bool isOnSlope;//var which indicates if wa are on flat floor or a slope
    //private bool isOnSlopeUp;//var which indicates if wa are on flat floor or a slope
    //private bool isOnSlopeDown;//var which indicates if wa are on flat floor or a slope
    private Vector2 slopeNormalPerp;//Perpendicular vector to the Normal one with the Floor
    private float slopeDownAngle;//Angle between the Normal vector and the +Y direction.
    private float slopeDownAngleOld;//Former value of slopeDownAngle

    [Header("Jump")]
    public float jumpForce;
    bool jumpPressed; //Tells me if I can jump or not

    Animator anim;
    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
    PlayerHealth playerHealth;
    Vector2 targetVelocity; //Speed I want to move the player
    Vector2 dampVelocity;//var. where I'm going to save the current speed of the player
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        //If Hurting Animation is running then won't execute the Update method
        if (anim.GetBool("Hurt")) return;

        InputPlayer();
        Flip();
        Animating();
        IsGrounded();
        //IsOnSlope();
        JumpPressed();
        //Attack();
        TargetVelocity();        
    }
    void FixedUpdate()
    {
        //Movement handling
        Move();
        //Jumping handling
        if (jumpPressed)
            Jump();
        ChangeGravity();
    }
    void Move()
    {
        rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, targetVelocity, ref dampVelocity, smoothTime);
    }
    void InputPlayer()
    {
        horizontal = Input.GetAxis("Horizontal");
    }
    void TargetVelocity()
    {
        targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y);
        //if (!isOnSlope)
        //    targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y);
        //else
        //    targetVelocity = new Vector2(horizontal * speed, rb2D.velocity.y*10);

    }
    void Animating()
    {
        //Running animation handling
        if (horizontal != 0) anim.SetBool("IsRunning", true);
        else anim.SetBool("IsRunning", false);
        //Jumping Animation handling (IsJumping param. will take the NOT value of isGrounded)
        anim.SetBool("IsJumping", !isGrounded);
    }
    //void Attack()
    //{
    //    //If I press the mouse button and the player is not moving
    //    if (Input.GetMouseButtonDown(0) && h == 0)
    //    {
    //        canMove = false;
    //        anim.SetTrigger("Attack");
    //    }
    //}
    void Jump()
    {
        jumpPressed = false;
        if (canMove)        
            rb2D.AddForce(Vector2.up * jumpForce);
    }
    void JumpPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) jumpPressed = true;
    }
    void IsGrounded()
    {
        //I launch a selective raycast (only detects the objects in the Ground Layer). 
        //It has rayLength as length, groundCheck as origin, down-Y direction.
        //The raycast return true if it is touching an object of the ground Layer
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(groundCheck.position, Vector2.down * rayLength, Color.red);
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
    void Flip()
    {
        if (horizontal < 0) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
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
    void CanMoveToTrue()
    {
        canMove = true;
    }
    public void ResetVelocity()
    {
        targetVelocity = Vector2.zero;
    }
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
}
