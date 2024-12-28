using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class HookManager : MonoBehaviour
{
    [Header("Grappling-Hook")]    
    // Grappling-Hook Rope Angle
    [SerializeField] private float ropeAngle;
    // Grappling-Hook Rope length
    [SerializeField] private float ropeLength;
    public float RopeLength { get { return ropeLength;} }
    // XY Rope Dist. coordinates
    private float ropeYDist, ropeXDist;
    // Grappling-Rope Starting & Ending Points
    private Vector3 startRopePoint = new Vector3(0, 0, 0);          // Line origin    
    private Vector3 endRopePoint;                                   // xº to the right or to the left      

    // Grappling Point position
    private Vector3 grapplingPointPos;
    public Vector3 GrapplingPointPos { get { return grapplingPointPos; } }

    // Rope Min, Max and 0 Positions (key Pendulum movement player pos)
    private Vector3 ropeMinPos, ropeMaxPos, rope0Pos;
    public Vector3 RopeMinPos { get { return ropeMinPos; } }
    public Vector3 RopeMaxPos { get { return ropeMaxPos; } }
    public Vector3 Rope0Pos { get { return rope0Pos; } }

    // Offset between the Player position & the Grappling-Hook position
    private Vector3 offsetHookPos;

    // Hook Enabled Flags
    private bool isHookEnabled;                                     // Is enabled as long as the Hook will be enabled
    public bool IsHookEnabled {  get { return isHookEnabled; } }

    private bool isHooked;                                          // Is enabled as long as a Grappling Point has been reached
    public bool IsHooked { get { return isHooked; } }

    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // GO components
    private LineRenderer lineRenderer;    
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private CapsuleCollider2D hookCollider2D;

    // Start is called before the first frame update
    void Awake()
    {
        // Get the Hook Capsule Collider 2D Component        
        hookCollider2D = GetComponent<CapsuleCollider2D>();

        // Get LineRenderer component from the Rope GO                
        lineRenderer = transform.parent?.Find("Rope")?.GetComponent<LineRenderer>();
        // Get the Sprite Renderer component from the player
        spriteRenderer = GameObject.FindWithTag("Player")?.GetComponent<SpriteRenderer>();        
        // Get the Player Movement component (script) from the player
        playerMovement = GameObject.FindWithTag("Player")?.GetComponent<PlayerMovement>();

        // Get the offset between the player pos. and the Grappling-Hook pos.
        offsetHookPos = transform.parent.localPosition;
    }    

    // Update is called once per frame
    void Update()
    {                       
        flipDirection = spriteRenderer.flipX ? -1 : 1;  // Updates the the player's flip direction var every frame
        
        if (playerMovement.CanEnableHook)
        {
            playerMovement.CanEnableHookToFalse();          // Reset the Can Enable Hook flag
            StartCoroutine(nameof(EnableGrapplingHook));   // Shows the LineRenderer of the Rope on its correct direction
                                                         // + Enables and set the offset of the CapsuleCollider2D of the Hook
        }
        
        //if (playerMovement.CurrentJumpState == PlayerMovement.JumpingState.Swinging)
        if(IsHooked)
            UpdateGrapplingHook();
    }
    // Updates the Line Renderer when the player is on the Swinging State
    void UpdateGrapplingHook()
    {        
        ///////////////////////////////////////////////////////////////////////////
        // LineRenderer (Rope) Angle RESPECT TO THE PLAYER WILL BE UPDATED HERE ///
        ///////////////////////////////////////////////////////////////////////////
    }
    // Enables the Grappling Hook elements
    IEnumerator EnableGrapplingHook()
    {
        // Calculates the Rope distance on the XY coordinates (As the angle = 45º are the same)
        ropeYDist = ropeLength * Mathf.Sin(ropeAngle * Mathf.Deg2Rad);
        ropeXDist = ropeLength * Mathf.Cos(ropeAngle * Mathf.Deg2Rad) * flipDirection;
        
        //Vector3 endPointRight = new Vector3(2.121f * flipDirection, 2.121f, 0);   
        endRopePoint = new Vector3(ropeXDist, ropeYDist, 0);                    // xº to the right or to the left

        // Set the LineRenderer Positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startRopePoint);                // Starting point 
        lineRenderer.SetPosition(1, endRopePoint);                  // Ending point (Initially to the right)                

        // Configura el ancho de la línea
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Configure the Hook offsets & Enable his CapsuleCollider2D
        hookCollider2D.offset = endRopePoint;
        hookCollider2D.enabled = true;

        isHookEnabled = true;                                   // Enables the Flag

        yield return new WaitForSeconds(0.7f);                     // Leave the Line Renderer visible for 2s

        // Hides The Line Renderer if the player is not on the Swinging State after 2s
        //if (playerMovement.CurrentJumpState != PlayerMovement.JumpingState.Swinging)        
        if (!IsHooked)
            DisableGrapplingHook();                    
    }
    // Disable the Grappling Hook elements
    public void DisableGrapplingHook()
    {
        isHookEnabled = false;                                   // Disable the Flags
        isHooked = false;

        // Hides the Rope (Line Renderer)
        lineRenderer.positionCount = 0;
        // Disables the Hook (Circle Collider 2D)
        hookCollider2D.enabled = false;
    }

    // Collisions Detections with the Grappling Points
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collision was made with the Grappling Point
        if (collision.CompareTag("GrapplingPoint"))
        {
            // Enables the flag 
            isHooked = true;

            // Play audio Grappling-Hook effect
            //audioHook.Play();

            // Gets the Grappling Point position --> This will be used on PlayerMovement to position the player             
            grapplingPointPos = collision.gameObject.transform.position;

            // Calculate also the Min, Max & 0 Positions. (Important to add the offset with the Player)
            ropeMinPos = new Vector3(grapplingPointPos.x - endRopePoint.x,
                                    grapplingPointPos.y - endRopePoint.y,
                                    0) - offsetHookPos;
            ropeMaxPos = new Vector3 (grapplingPointPos.x + endRopePoint.x,
                                    grapplingPointPos.y - endRopePoint.y,
                                    0) - offsetHookPos;
            rope0Pos = new Vector3(grapplingPointPos.x ,
                                    grapplingPointPos.y - ropeLength,
                                    0) - offsetHookPos;            

            // Debugging
            Debug.Log("Gancho atrapado en el punto de agarre");
        }        
    }
}
