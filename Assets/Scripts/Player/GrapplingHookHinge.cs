using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class GrapplingHookHinge : MonoBehaviour
{
    [Header("Grappling-Hook")]
    private GameObject grapplingHookPivot;                      // GameObject hijo que contiene el HingeJoint2D    
    [SerializeField] private LayerMask grapplableLayer;         // Capas donde se puede enganchar el gancho        
    [SerializeField] private float ropeAngle;                   // Grappling-Hook Rope Angle    
    [SerializeField] private float ropeLength;                  // Grappling-Hook Rope length
    public float RopeLength { get { return ropeLength; } }
    [SerializeField] private float raycastDuration;

    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // Rope Vectors
    Vector2 ropeDirection;
    Vector2 endRopePoint;
    Vector2 jointPoint;

    // Rope Distance (Once the player is hooked)
    [SerializeField] private float ropeSwingLength;

    // Fading Grappling-Hook
    float elapsedRopeTime = 0f;
    float fadingElapsedTime = 0f;
    float fadingFactor = 0f;
    private Color startColor;             // Color inicial del LineRenderer
    private Color endColor;               // Color final (transparente)

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
    public bool DistanceJointIsEnabled { get { return distanceJoint.enabled; } }
    public Vector2 DistanceJointConnAnchor { get { return distanceJoint.connectedAnchor; } }

    // Start is called before the first frame update
    void Start()
    {
        // Get the Player Movement script
        playerMovement = GetComponent<PlayerMovement>();
        // Get the Sprite Renderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Obtener el Rigidbody2D del jugador (en este caso, el GameObject padre)
        playerRigidbody = GetComponent<Rigidbody2D>();

        // Get the Hook Pivot Rigidbody Component and Configure it        
        //ConfigurePivotRigidBody();
        // Configure the Hinge Joint 2D
        //ConfigureHingeJoint();
        // Create and set initial settings to a Distance Joint Component                
        ConfigureDistanceJoint();                
        // Configure the Line Renderer Component
        ConfigureLineRenderer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        flipDirection = spriteRenderer.flipX ? -1 : 1;  // Updates the the player's flip direction var every frame

        if (playerMovement.CanEnableHook)
        {
            playerMovement.CanEnableHookToFalse();          // Reset the Can Enable Hook flag
            EnableGrapplingHook();                          // Shows the LineRenderer of the Rope on its correct direction                                                            
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
        distanceJoint = GetComponent<DistanceJoint2D>();
        distanceJoint.autoConfigureDistance = false;
        distanceJoint.enabled = false;        
    }
    // Enable the DistanceJoint2D
    void EnableDistanceJoint(Vector2 jointPoint)
    {
        distanceJoint.connectedAnchor = jointPoint;     // Punto de conexión
        distanceJoint.distance = ropeSwingLength;      // Rope lenght while swinging (2 uds.)       
        distanceJoint.enabled = true;                   // Activar el DistanceJoint2D        
    }
    void ConfigureHingeJoint()
    {
        hingeJoint = grapplingHookPivot.GetComponent<HingeJoint2D>();
        hingeJoint.enabled = false; // Desactivado inicialmente
        hingeJoint.autoConfigureConnectedAnchor = false; // Configuración manual del punto de conexión
        hingeJoint.connectedBody = pivotRigidbody;
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
        // Set the initial colors as black and visible
        lineRenderer.startColor = new Color(0f,0f,0f,1f);
        lineRenderer.endColor = new Color(0f,0f,0f,1f);
        // Desactivar la cuerda al inicio
        lineRenderer.enabled = false;
    }
    // Enable the Line Renderer position
    void EnableLineRenderer(Vector2 endPoint)
    {                
        // Fading Starting colors (transparency)                 
        //startColor = new Color(0f, 0f, 0f, 0f);                 // The start color will be completely transparent
        //endColor = new Color(0f, 0f, 0f, 1f);                   // The end color will be completely transparent
        //// Set the initial colors as black and transparent
        //lineRenderer.startColor = startColor;
        //lineRenderer.endColor = startColor;

        // Activar y configurar el LineRenderer para la cuerda visual
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, playerRigidbody.position);  // Punto de inicio (jugador)
        //lineRenderer.SetPosition(1, endPoint);                  // Punto final (enganche)
    }

    ///////////////////////////////////
    // RAYCAST & CALCULATION METHODS //
    ///////////////////////////////////

    // Rope Vectors calculation
    void CalculateRopeVectors()
    {
        // Calculates the Rope distance on the XY coordinates (As the angle = 45º are the same)
        float ropeYDist = ropeLength * Mathf.Sin(ropeAngle * Mathf.Deg2Rad);
        float ropeXDist = ropeLength * Mathf.Cos(ropeAngle * Mathf.Deg2Rad) * flipDirection;
        //Vector3 endPointRight = new Vector3(2.121f * flipDirection, 2.121f, 0);   

        // Gets the Rope Direction normalized according to the Rope Length and Angle (Set on the Inspector)
        ropeDirection = new Vector2(ropeXDist, ropeYDist).normalized;
        // Gets the End Point of the rope (Hook point)
        endRopePoint = playerRigidbody.position + (ropeDirection * ropeLength * fadingFactor);
    }
    //void DetectGrapplingJoint_(out RaycastHit2D hit, Vector2 ropeDirection)
    //{
    //    // Detectar si hay un punto de enganche dentro del rango
    //    hit = Physics2D.Raycast(playerRigidbody.position, ropeDirection, ropeLength, grapplableLayer);
    //    // Raycast debugging
    //    Debug.DrawRay(playerRigidbody.position, ropeDirection * ropeLength, Color.red);
    //}

    // Launch a Raycast during a certain time in order to detect Hook points
    IEnumerator DetectGrapplingJoint()
    {        
        // Launch a Raycast during the amount of seconds defined on raycastDuration
        elapsedRopeTime = 0f;
        fadingFactor = 0f;
        while (elapsedRopeTime < raycastDuration)
        {
            // Update the Fading Factor
            //FadingGrapplingHook(elapsedRopeTime);

            // Detectar si hay un punto de enganche dentro del rango
            RaycastHit2D hit = Physics2D.Raycast(playerRigidbody.position, ropeDirection, fadingFactor*ropeLength, grapplableLayer);
            // Raycast debugging
            Debug.DrawRay(playerRigidbody.position, ropeDirection * (fadingFactor * ropeLength), Color.red);

            //Check collision
            if (hit.collider != null)
            {
                // Get the central point of the Joint Point                
                jointPoint = (Vector2)hit.collider.gameObject.transform.position;

                // Enable the DistanceJoint2D            
                //EnableDistanceJoint(hit.point);
                EnableDistanceJoint(jointPoint);
                // Update the ending point as the hook point (Only if succesful hooking)           
                //lineRenderer.SetPosition(1, hit.point);
                lineRenderer.SetPosition(1, jointPoint);
                // Enables the flag which indicates the Hooking was successful
                isHooked = true;                
                // Stops the coroutine
                yield break; 
            }
            //Increase timer
            elapsedRopeTime += Time.deltaTime;
            // Waits till the next frame
            yield return null;
        }
        // If Raycast Duration elapsed and no Hook point has been detected then
        // the Grappling Hook will be disabled (Line Renderer + Distance Joint 2D + Hook flags vars.)                
        if (!IsHooked)
            DisableGrapplingHook();
    }
    void FadingGrapplingHook(float elapsedTime)
    {        
        // Update the fading Factor
        //fadingElapsedTime += elapsedTime;
        fadingFactor = elapsedTime / raycastDuration;

        // fadingFactor Limitation
        if (fadingFactor >= 1)
            fadingFactor = 1;
        
        //// Apply the fading effect to the LineRenderer color
        //Color currentColor = Color.Lerp(startColor, endColor, fadingFactor);  
        //lineRenderer.startColor = currentColor;  
        //lineRenderer.endColor = currentColor;

        // Ajustar la longitud de la cuerda (LineRenderer) gradualmente
        //lineRenderer.SetPosition(1, playerRigidbody.position + ropeDirection * (fadingFactor * ropeLength));
        //lineRenderer.SetPosition(1, fadingFactor * endRopePoint);
    }
    void UpdateLineRenderer()
    {
        // Apply the fading effect to the LineRenderer color
        //Color currentColor = Color.Lerp(startColor, endColor, fadingFactor);
        //lineRenderer.startColor = currentColor;
        //lineRenderer.endColor = currentColor;

        // Update the Line Renderer depending on the Hooking State
        lineRenderer.SetPosition(0, playerRigidbody.position);          // Starting Point (Player's rb)
        if (isHooked)
            lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);    // Joint Point
        else
            lineRenderer.SetPosition(1, endRopePoint);                  // Rope Direction   
    }

    ////////////////////////////
    // GRAPPLING-HOOK METHODS //
    ////////////////////////////
    // Updates the Line Renderer when the player is on the Swinging State
    void UpdateGrapplingHook()
    {
        // Update the Fading Factor
        if (fadingFactor<1)
            FadingGrapplingHook(elapsedRopeTime);

        // Rope Vectors calculation        
        CalculateRopeVectors();

        // Update the Line Renderer depending on the Hooking State
        UpdateLineRenderer();

        //// Update the Line Renderer depending on the Hooking State
        //lineRenderer.SetPosition(0, playerRigidbody.position);          // Starting Point (Player's rb)
        //if (isHooked)
        //    lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);    // Joint Point
        //else
        //    lineRenderer.SetPosition(1, endRopePoint);                  // Rope Direction                                                                                       
    }
    // Enables the Grappling Hook elements
    public void EnableGrapplingHook()
    {
        // Enables the flag which indicates the Hook has been thrown
        isHookEnabled = true;

        // Rope Vectors calculation        
        CalculateRopeVectors();

        // Activar y configurar el LineRenderer para la cuerda visual        
        EnableLineRenderer(endRopePoint);

        // Raycast launching to detect the Joint        
        StartCoroutine(nameof(DetectGrapplingJoint));                                  
    }
    // Disable the Grappling Hook elements
    public void DisableGrapplingHook()
    {
        // Disable the Hook flags
        isHookEnabled = false;                                   
        isHooked = false;
        
        //hingeJoint.enabled = false;     // Disable the Hinge Joint
        lineRenderer.enabled = false;   // Disable the visual Rope (Line Renderer)
        distanceJoint.enabled = false;  // Disable the Distance Joint
    }    
}
