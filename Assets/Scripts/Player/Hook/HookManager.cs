using System.Collections;
using System.Collections.Generic;
using System.Net;
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

    // Swinging Movement (Pendulum) vars.   
    private float pendulumAmp;              // Min amplitude (x_0)
    public float PendulumAmp {get { return pendulumAmp; } }

    private float pendulumPhi0;             // Angle between the vertical and the rope (Phi_0)
    public float PendulumPhi0 { get { return pendulumPhi0; } }

    //private float pendulumLength;         // Rope Length (L) = ropeLength

    private float pendulumGravity = 9.8f;   //9.8f;   // Gravity (g)
    public float PendulumGravity { get { return pendulumGravity; } }

    private float pendulumPhase = 0.0f;     // Initial Phase (phi)
    public float PendulumPhase { get { return pendulumPhase; } }

    private float pendulumOmega;            // Angular frequency (omega)
    public float PendulumOmega { get { return pendulumOmega; } }    

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

        // Position difference between the player and the Grappling Point
        Vector2 diffHookPlayer = grapplingPointPos - playerMovement.transform.position;

        // Angle between the Rope and the player
        float alpha = Mathf.Atan2(diffHookPlayer.y, diffHookPlayer.x + offsetHookPos.x);

        // Calculate the rope end position
        Vector2 endPoint = new Vector2(Mathf.Cos(alpha) * ropeLength,Mathf.Sin(alpha) * ropeLength);
        
        // Global position of the end of the rope.
        //Vector2 point1 = (Vector2)playerMovement.transform.position + endPoint;

        // Update the LineRenderer 1st Position
        lineRenderer.SetPosition(1, endPoint);

        // Update the Hook Collider position
        hookCollider2D.offset = endPoint;
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

        // Reset the Hook Collider position
        hookCollider2D.offset = Vector2.zero;
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

            // Set the Pendulum vars.            
            pendulumPhi0 = (180 - 90 - ropeAngle) * (-flipDirection) * Mathf.Deg2Rad;   // On radians
            pendulumAmp = ropeLength * Mathf.Sin(pendulumPhi0);                         // L*sin(Phi_0)            

            pendulumOmega = Mathf.Sqrt(pendulumGravity / ropeLength);   // Omega = Sqrt(g/L)            

            // Debugging
            Debug.Log("Gancho atrapado en el punto de agarre");
        }        
    }
}
