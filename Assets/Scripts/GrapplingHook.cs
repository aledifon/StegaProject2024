using UnityEngine;
using System.Collections;

public class GrapplingHook : MonoBehaviour
{
    [Header("Grappling-Hook")]
    //private GameObject grapplingHookPivot;                      // GameObject hijo que contiene el HingeJoint2D    
    [SerializeField] private LayerMask grapplableLayer;         // Capas donde se puede enganchar el gancho        
    [SerializeField] private float ropeAngle;                   // Grappling-Hook Rope Angle    
    [SerializeField] private float ropeLength;                  // Grappling-Hook Rope length
    public float RopeLength { get { return ropeLength; } }    

    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // Rope Vectors
    Vector2 ropeDirection;
    Vector2 endRopePoint;
    Vector2 jointPoint;

    // Tangential Rope Force
    private Vector2 tangentialForce;
    public Vector2 TangentialForce { get { return tangentialForce; } }

    // Current Rope Angle (Between the Rope and the player)
    private float currentRopeAngle;
    public float CurrentRopeAngle { get { return currentRopeAngle; } }

    private float maxRopeAngle;
    public float MaxRopeAngle { get { return maxRopeAngle; } }
    private float minRopeAngle;
    public float MinRopeAngle { get { return minRopeAngle; } }

    private bool isWithinAngleLimits;
    public bool IsWithinAngleLimits { get { return isWithinAngleLimits; } }


    // Rope Distance (Once the player is hooked)
    [SerializeField] private float ropeSwingLength;

    // Fading Grappling-Hook
    private float elapsedHookThrownTime;
    private float hookThrownMaxTime;
    float fadingFactor = 0f;
    private Color startColor;             // Color inicial del LineRenderer
    private Color endColor;               // Color final (transparente)

    // Hook Enabled Flags    
    private bool isHookAttached;                                          // Is enabled as long as a Grappling Point has been reached
    public bool IsHookAttached { get { return isHookAttached; } }

    // GO components    
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;       // Línea visual del gancho
    //private HingeJoint2D hingeJoint;
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D playerRigidbody;
    private Rigidbody2D pivotRigidbody;
    private PlayerMovement playerMovement;    

    // Hinge Joint 2D Propeties
    public bool DistanceJointIsEnabled { get { return distanceJoint.enabled; } }
    public Vector2 DistanceJointConnAnchor { get { return distanceJoint.connectedAnchor; } }

    #region Unity API
    private void OnEnable()
    {
        playerMovement.OnHookThrown += TriggerGrapplingHook;
        playerMovement.OnHookPickUp += DisableGrapplingHook;
    }
    private void OnDisable()
    {
        playerMovement.OnHookThrown -= TriggerGrapplingHook;
        playerMovement.OnHookPickUp -= DisableGrapplingHook;
    }
    void Awake()
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
        // Update visually the rope in every frame        
        //if (isHookEnabled)
        if (playerMovement.IsHookThrownEnabled || isHookAttached)
        {
            // Updates the the player's flip direction var every frame          
            flipDirection = spriteRenderer.flipX ? -1 : 1;  
            // Update the HookThrownTimer
            elapsedHookThrownTime = (playerMovement.HookThrownMaxTime - playerMovement.HookThrownTimer);
            hookThrownMaxTime = playerMovement.HookThrownMaxTime;

            UpdateGrapplingHook();
        }            
    }
    #endregion    
    #region Components Setup
    // Create and set initial settings to a Distance Joint Component
    // (It will allow to keep a fixed distance between the player and the Joint Point)
    void ConfigureDistanceJoint()
    {
        distanceJoint = GetComponent<DistanceJoint2D>();
        distanceJoint.autoConfigureDistance = false;
        distanceJoint.enabled = false;
    }
    // Enable the DistanceJoint2D
    //void EnableDistanceJoint(Vector2 jointPoint)
    void EnableDistanceJoint(Rigidbody2D pivotRigidbody)
    {
        //distanceJoint.connectedAnchor = jointPoint;     // Punto de conexión

        distanceJoint.connectedBody = pivotRigidbody;       // Links the "connectedBody" (the joint point) to the "pivotRigidbody" (a fixed GO)                
        distanceJoint.connectedAnchor = new Vector2(0, 0);  // Set the point where the player connects with the rope
        distanceJoint.distance = ropeSwingLength;           // Set the max. rope distance (while swinging (2 uds.))
        distanceJoint.enabled = true;                       // Enables the DistanceJoint2D        
    }
    //void ConfigureHingeJoint()
    //{
    //    hingeJoint = grapplingHookPivot.GetComponent<HingeJoint2D>();
    //    hingeJoint.enabled = false; // Desactivado inicialmente
    //    hingeJoint.autoConfigureConnectedAnchor = false; // Configuración manual del punto de conexión
    //    hingeJoint.connectedBody = pivotRigidbody;
    //}
    // Enable the HingeJoint2D y hook point setting
    //void EnableHingeJoint(Vector2 hitPoint)
    //{        
    //    hingeJoint.enabled = true;
    //    hingeJoint.connectedAnchor = hitPoint;
    //}
    // Get the Hook Pivot Rigidbody Component and Configure it
    void ConfigurePivotRigidBody()
    {
        //pivotRigidbody = grapplingHookPivot.GetComponent<Rigidbody2D>();
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
        lineRenderer.startColor = new Color(0f, 0f, 0f, 1f);
        lineRenderer.endColor = new Color(0f, 0f, 0f, 1f);
        // Set only 1 point on the Line Renderer by default
        lineRenderer.positionCount = 1;
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
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, playerRigidbody.position);  // Punto de inicio (jugador)
        lineRenderer.SetPosition(1, endPoint);                  // Punto final (enganche)
        lineRenderer.enabled = true;        
    }
    #endregion    
    #region Calculation
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
    // Calculate the TangentialForce to be applied to keep the armonic movement.
    void CalculateTangentialForce()
    {
        //tangentialForce = Vector2.Perpendicular(distanceJoint.connectedAnchor - (Vector2)playerRigidbody.position).normalized;

        // Calculate the direction between the joint and the player
        Vector2 directionToAnchor = distanceJoint.connectedBody.position - (Vector2)playerRigidbody.position;
        // Normalized the rope direction (get only the direction without the magnitude)
        Vector2 normalizedRopeDirection = directionToAnchor.normalized;

        // Get the Angle between the rope and the player
        currentRopeAngle = CalculateRopeAngle(normalizedRopeDirection);

        // Update the tangentialForce in func. of the current Rope Angle
        if (currentRopeAngle > minRopeAngle && currentRopeAngle < 90)
            tangentialForce = -Vector2.Perpendicular(normalizedRopeDirection);
        else if (currentRopeAngle > 90 && currentRopeAngle < maxRopeAngle)
            tangentialForce = Vector2.Perpendicular(normalizedRopeDirection);
        else if (currentRopeAngle == 90 || currentRopeAngle > maxRopeAngle || currentRopeAngle < minRopeAngle)
            tangentialForce = Vector2.zero;

        // Check if the Rope Angle is within the limits
        isWithinAngleLimits = (currentRopeAngle >= (minRopeAngle + 10) && currentRopeAngle <= (maxRopeAngle - 10));

        // Visualizar la dirección tangencial
        Debug.DrawRay(playerRigidbody.position, tangentialForce, Color.magenta);
        //Debug.Log("Tangential Force = " + tangentialForce);
    }
    float CalculateRopeAngle(Vector2 normalizedRopeDirection)
    {
        // Get the angle between the rope and the horizontal (Escalar product)
        float dotProduct = Vector2.Dot(normalizedRopeDirection, Vector2.right);
        // Get the angle (from the trigon. relation cos(phi)=AxB; phi = acos(AxB))
        float angleInRadians = Mathf.Acos(dotProduct);
        // Angle 2 rads conversion
        float angleInDegrees = Mathf.Rad2Deg * angleInRadians;
        // Debugging
        Debug.Log("Ángulo entre la cuerda y la horizontal: " + angleInDegrees);
        return angleInDegrees;
    }
    #endregion
    #region Grappling Hook
    #region Visual
    void FadingGrapplingHook(float elapsedTime, float totalDuration)
    {
        // Update the fading Factor
        //fadingElapsedTime += elapsedTime;
        fadingFactor = elapsedTime / totalDuration;

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
        if (isHookAttached)
            //lineRenderer.SetPosition(1, distanceJoint.connectedAnchor);    // Joint Point
            lineRenderer.SetPosition(1, distanceJoint.connectedBody.position);    // Joint Point
        else
            lineRenderer.SetPosition(1, endRopePoint);                  // Rope Direction   
    }
    #endregion
    IEnumerator DetectGrapplingJoint()
    {
        yield return new WaitUntil (() => playerMovement.IsHookThrownEnabled);

        // Raycast number and the scanning angle
        int raycastCount = 10;
        float raycastAngleRange = 45f; // Total angle on degrees (i.e.: 45 degrees around the main direction)        

        // Launch a bunch of Raycast during the amount of seconds defined on raycastDuration        
        while (playerMovement.IsHookThrownEnabled)
        {
            // Initial angle calculation
            float baseAngle = Mathf.Atan2(ropeDirection.y, ropeDirection.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - raycastAngleRange / 2;
            float angleStep = raycastAngleRange / (raycastCount - 1);

            for (int i = 0; i < raycastCount; i++)
            {
                // Calculate the current raycast direction
                float currentAngle = startAngle + angleStep * i;
                Vector2 currentDirection = new Vector2(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad)
                );

                //// Detectar si hay un punto de enganche dentro del rango
                //RaycastHit2D hit = Physics2D.Raycast(playerRigidbody.position, ropeDirection, fadingFactor * ropeLength, grapplableLayer);
                //// Raycast debugging
                //Debug.DrawRay(playerRigidbody.position, ropeDirection * (fadingFactor * ropeLength), Color.red);

                // Detectar si hay un punto de enganche dentro del rango
                RaycastHit2D hit = Physics2D.Raycast(playerRigidbody.position, currentDirection, fadingFactor * ropeLength, grapplableLayer);
                // Raycast debugging
                Debug.DrawRay(playerRigidbody.position, currentDirection * (fadingFactor * ropeLength), Color.red);

                //Check collision
                if (hit.collider != null)
                {
                    // Get the Joint GO Rigidbody & Configure it
                    pivotRigidbody = hit.collider.GetComponent<Rigidbody2D>();
                    if (pivotRigidbody != null)
                    {
                        ConfigurePivotRigidBody();
                        EnableDistanceJoint(pivotRigidbody);
                    }
                    else
                    {
                        Debug.LogWarning("El objeto enganchado no tiene un Rigidbody2D.");
                        continue; // Continúa buscando un punto válido
                    }

                    // Get the central point of the Joint Point                
                    jointPoint = (Vector2)hit.collider.gameObject.transform.position;

                    // Enable the DistanceJoint2D            
                    //EnableDistanceJoint(hit.point);
                    //EnableDistanceJoint(jointPoint);                                        

                    // Update the ending point as the hook point (Only if succesful hooking)           
                    //lineRenderer.SetPosition(1, hit.point);
                    lineRenderer.SetPosition(1, jointPoint);
                    // Enables the flag which indicates the Hooking was successful
                    isHookAttached = true;
                    // Stops the coroutine
                    yield break;
                }
            }

            // Waits till the next frame
            yield return null;
        }
    }
    // Updates the Line Renderer when the player is on the Swinging State
    void UpdateGrapplingHook()
    {
        // Update the Fading Factor
        if (fadingFactor < 1)
            FadingGrapplingHook(elapsedHookThrownTime, hookThrownMaxTime);

        // Rope Vectors calculation        
        CalculateRopeVectors();

        // Update the Line Renderer depending on the Hooking State
        UpdateLineRenderer();

        // Calculate the Tangential Force
        if (isHookAttached)
            CalculateTangentialForce();
    }
    #region Enable/Disable GrapplingHook
    // Enables the Grappling Hook elements
    public void TriggerGrapplingHook()
    {        
        // Calulate min & max angles
        minRopeAngle = 90 - 60;
        maxRopeAngle = 90 + 60;

        // Reset the Fading Factor value        
        fadingFactor = 0f;

        // Set the current player's flip direction
        flipDirection = spriteRenderer.flipX ? -1 : 1;

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
        // Disable the Hook flag        
        isHookAttached = false;        

        //hingeJoint.enabled = false;     // Disable the Hinge Joint
        lineRenderer.enabled = false;   // Disable the visual Rope (Line Renderer)
        distanceJoint.enabled = false;  // Disable the Distance Joint
    }
    #endregion
    #endregion
}
