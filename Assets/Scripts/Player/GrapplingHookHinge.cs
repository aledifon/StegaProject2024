using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class GrapplingHookHinge : MonoBehaviour
{
    [Header("Grappling-Hook")]
    [SerializeField] private GameObject grapplingHookPivot;     // GameObject hijo que contiene el HingeJoint2D    
    [SerializeField] private LayerMask grapplableLayer;         // Capas donde se puede enganchar el gancho        
    [SerializeField] private float ropeAngle;                   // Grappling-Hook Rope Angle    
    [SerializeField] private float ropeLength;                  // Grappling-Hook Rope length
    public float RopeLength { get { return ropeLength; } }    

    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // Hook Enabled Flags
    private bool isHookEnabled;                                     // Is enabled as long as the Hook will be enabled
    public bool IsHookEnabled { get { return isHookEnabled; } }

    private bool isHooked;                                          // Is enabled as long as a Grappling Point has been reached
    public bool IsHooked { get { return isHooked; } }    

    // GO components    
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;       // Línea visual del gancho
    private HingeJoint2D hingeJoint;
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D playerRigidbody;
    private Rigidbody2D pivotRigidbody;
    private PlayerMovement playerMovement;
    private Camera mainCamera;

    // Hinge Joint 2D Propeties
    public bool HingeJointIsEnabled { get { return hingeJoint.enabled; } }
    public Vector2 HingeJointConnAnchor { get { return hingeJoint.connectedAnchor; } }

    // Start is called before the first frame update
    void Start()
    {
        // Get the Player Movement script
        playerMovement = GetComponent<PlayerMovement>();
        // Get the Sprite Renderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Obtener el Rigidbody2D del jugador (en este caso, el GameObject padre)
        playerRigidbody = GetComponent<Rigidbody2D>();

        // Configure the Hinge Joint 2D
        ConfigureHingeJoint();
        // Create and set initial settings to a Distance Joint Component                
        ConfigureDistanceJoint();
        // Get the Hook Pivot Rigidbody Component and Configure it        
        ConfigurePivotRigidBody();        
        // Configure the Line Renderer Component
        ConfigureLineRenderer();

        // Obtener la cámara principal
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        flipDirection = spriteRenderer.flipX ? -1 : 1;  // Updates the the player's flip direction var every frame

        if (playerMovement.CanEnableHook)
        {
            playerMovement.CanEnableHookToFalse();          // Reset the Can Enable Hook flag
            StartCoroutine(nameof(EnableGrapplingHook));   // Shows the LineRenderer of the Rope on its correct direction
                                                           // + Enables and set the offset of the CapsuleCollider2D of the Hook
        }

        //// Disparar el gancho al hacer clic
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //    // Detectar si hay un punto de enganche dentro del rango
        //    RaycastHit2D hit = Physics2D.Raycast(playerRigidbody.position, mousePosition - (Vector2)playerRigidbody.position, maxDistance, grapplableLayer);

        //    if (hit.collider != null)
        //    {
        //        // Activar el HingeJoint2D y configurar el punto de enganche
        //        hingeJoint.enabled = true;
        //        hingeJoint.connectedAnchor = hit.point;

        //        // Activar y configurar el LineRenderer para la cuerda visual
        //        lineRenderer.enabled = true;
        //        lineRenderer.SetPosition(0, playerRigidbody.position); // Punto de inicio (jugador)
        //        lineRenderer.SetPosition(1, hit.point);               // Punto final (enganche)
        //    }
        //}
        //// Soltar el gancho al soltar el clic
        //if (Input.GetMouseButtonUp(0))
        //{
        //    hingeJoint.enabled = false;  // Desactivar el joint
        //    lineRenderer.enabled = false; // Desactivar la cuerda visual
        //}        

        // Update visually the rope in every frame
        //if (playerMovement.CurrentJumpState == PlayerMovement.JumpingState.Swinging)
        //if (IsHooked)
        if (isHookEnabled)        
            UpdateGrapplingHook();
    }    

    /////////////////////////////////
    // SETTINGS COMPONENTS METHODS //
    /////////////////////////////////
    
    // Create and set initial settings to a Distance Joint Component
    // (It will allow to keep a fixed distance between the player and the Joint Point)
    void ConfigureDistanceJoint()
    {
        distanceJoint = grapplingHookPivot.AddComponent<DistanceJoint2D>();
        distanceJoint.autoConfigureDistance = false;
        distanceJoint.enabled = false;
    }
    // Enable the DistanceJoint2D
    void EnableDistanceJoint(Vector2 hitPoint)
    {        
        distanceJoint.connectedAnchor = hitPoint;  // Punto de conexión
        distanceJoint.distance = ropeLength;        // Longitud de la cuerda
        distanceJoint.enabled = true;               // Activar el DistanceJoint2D
    }
    void ConfigureHingeJoint()
    {
        hingeJoint = grapplingHookPivot.GetComponent<HingeJoint2D>();
        hingeJoint.enabled = false; // Desactivado inicialmente
        hingeJoint.autoConfigureConnectedAnchor = false; // Configuración manual del punto de conexión
    }
    // Enable the HingeJoint2D y hook point setting
    void EnableHingeJoint(Vector2 hitPoint)
    {        
        hingeJoint.enabled = true;
        hingeJoint.connectedAnchor = hitPoint;
    }
    // Get the Hook Pivot Rigidbody Component and Configure it
    void ConfigurePivotRigidBody()
    {        
        pivotRigidbody = grapplingHookPivot.GetComponent<Rigidbody2D>();
        pivotRigidbody.bodyType = RigidbodyType2D.Kinematic;
        pivotRigidbody.gravityScale = 0;
        pivotRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    // Configure the Line Renderer (Rope)
    void ConfigureLineRenderer()
    {
        // Get the Line Renderer Component
        lineRenderer = GetComponent<LineRenderer>();
        // Configura el ancho de la línea
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        // Desactivar la cuerda al inicio
        lineRenderer.enabled = false;
    }
    // Enable the Line Renderer position
    void EnableLineRenderer(Vector2 endPoint)
    {
        // Activar y configurar el LineRenderer para la cuerda visual
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, playerRigidbody.position);  // Punto de inicio (jugador)
        lineRenderer.SetPosition(1, endPoint);                  // Punto final (enganche)
    }

    ///////////////////////////////////
    // RAYCAST & CALCULATION METHODS //
    ///////////////////////////////////
    
    // Rope Vectors calculation
    void CalculateRopeVectors(out Vector2 ropeDir, out Vector2 endPoint)
    {
        // Calculates the Rope distance on the XY coordinates (As the angle = 45º are the same)
        float ropeYDist = ropeLength * Mathf.Sin(ropeAngle * Mathf.Deg2Rad);
        float ropeXDist = ropeLength * Mathf.Cos(ropeAngle * Mathf.Deg2Rad) * flipDirection;
        //Vector3 endPointRight = new Vector3(2.121f * flipDirection, 2.121f, 0);   

        // Gets the Rope Direction normalized according to the Rope Length and Angle (Set on the Inspector)
        ropeDir = new Vector2(ropeXDist, ropeYDist).normalized;   
        // Gets the End Point of the rope (Hook point)
        endPoint = playerRigidbody.position + (ropeDir * ropeLength);
    }
    void DetectGrapplingJoint(out RaycastHit2D hit, Vector2 ropeDirection)
    {
        // Detectar si hay un punto de enganche dentro del rango
        hit = Physics2D.Raycast(playerRigidbody.position, ropeDirection, ropeLength, grapplableLayer);
        // Raycast debugging
        Debug.DrawRay(playerRigidbody.position, ropeDirection * ropeLength, Color.red);
    }

    ////////////////////////////
    // GRAPPLING-HOOK METHODS //
    ////////////////////////////
    // Updates the Line Renderer when the player is on the Swinging State
    void UpdateGrapplingHook()
    {
        // Rope Vectors calculation
        Vector2 ropeDirection, endRopePoint;
        CalculateRopeVectors(out ropeDirection, out endRopePoint);

        // Update the Line Renderer depending on the Hooking State
        lineRenderer.SetPosition(0, playerRigidbody.position);          // Starting Point (Player's rb)
        if (isHooked)
            lineRenderer.SetPosition(1, hingeJoint.connectedAnchor);    // Joint Point
        else
            lineRenderer.SetPosition(1, endRopePoint);                  // Rope Direction            
    }
    // Enables the Grappling Hook elements
    IEnumerator EnableGrapplingHook()
    {
        // Rope Vectors calculation        
        CalculateRopeVectors(out Vector2 ropeDirection,out Vector2 endRopePoint);

        // Raycast launching to detect the Joint
        DetectGrapplingJoint(out RaycastHit2D hit, ropeDirection);       

        // Activar y configurar el LineRenderer para la cuerda visual        
        EnableLineRenderer(endRopePoint);                                                            

        // Enables the flag which indicates the Hook has been thrown
        isHookEnabled = true;                                   

        if (hit.collider != null)
        {
            // Enable the HingeJoint2D y hook point configuration
            EnableHingeJoint(hit.point);
            // Enable the DistanceJoint2D            
            EnableDistanceJoint(hit.point);
            // Update the ending point as the hook poin (Only if succesful hooking)           
            lineRenderer.SetPosition(1, hit.point);               
            // Enables the flag which indicates the Hooking was successful
            isHooked = true;
        }

        // Leaves the Line Renderer visible for at least x seconds
        yield return new WaitForSeconds(0.7f);                  

        // Hides The Line Renderer if the player is not on the Swinging State after 2s
        //if (playerMovement.CurrentJumpState != PlayerMovement.JumpingState.Swinging)        
        if (!IsHooked)
            DisableGrapplingHook();
    }
    // Disable the Grappling Hook elements
    public void DisableGrapplingHook()
    {
        // Disable the Hook flags
        isHookEnabled = false;                                   
        isHooked = false;
        
        hingeJoint.enabled = false;     // Disable the Hinge Joint
        lineRenderer.enabled = false;   // Disable the visual Rope (Line Renderer)
        distanceJoint.enabled = false;  // Disable the Distance Joint
    }
}
