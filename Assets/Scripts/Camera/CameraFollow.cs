using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Refs")]
    Transform player;
    Transform cameraTarget;           

    [Header("Offsets")]
    [SerializeField] float xOffset = 1.5f;      // Horizontal offset acoording the player's dir
    [SerializeField] float yOffset = 0f;        // Vertical base offset
    [SerializeField] float zOffset = -10f;      // Z-Offset (Camera distance)

    [Header("Dynamic Factors")]
    public float dynamXFactor = 0.05f;          // To anticipate x speed
    public float dynamYFactor = 0.05f;          // To anticipate y speed
    public float dynamYExpFactor;              // To create an exp. curve

    [Header("Impulse")]
    [SerializeField] private CinemachineImpulseSource impulseSource;    
    [SerializeField,Range(0f,50f)] private float impulseForceFactor;                  //1.5f       

    [Header("Boundaries")]
    [SerializeField] CamBoundariesTriggerArea TotalBoundsArea;
    private CinemachineConfiner2D confiner2D;
    [SerializeField] CamBoundariesTriggerArea currentBoundsArea;
    [SerializeField] CamBoundariesTriggerArea lastBoundsArea;

    // Refs.    
    private PlayerMovement playerMovement;
    private SpriteRenderer playerSprite;
    private PlayerHealth playerHealth;
    private CinemachinePositionComposer composer;

    // Control vars
    private bool cameraFollowEnabled = true;

    #region Unity API
    private void Awake()
    {
        composer = GetComponent<CinemachinePositionComposer>();
        if (composer == null)
            Debug.LogError("Cinemachine Position Composer component not found on the GO " + gameObject + "!!");

        confiner2D = GetComponent<CinemachineConfiner2D>();
        if (confiner2D == null)
            Debug.LogError("Cinemachine Confiner 2D component not found on the GO " + gameObject + "!!");
    }
    private void Start()
    {
        // Set the initial Cam Boundaries
        //if(player != null)
        //    SetInitialBoundaries();
    }
    private void LateUpdate()
    {
        // Check that we have all the neede refs.
        if (player == null || cameraTarget == null || 
            playerSprite == null || playerMovement == null || playerHealth == null)
            return;

        // Get the Player's view direction
        int playerDir = playerSprite.flipX ? -1 : 1;

        // Set the X-Dynamic Offset 
        float dynX = xOffset * playerDir + playerMovement.rb2DPlayerVelX * dynamXFactor;
        // Set the Y-Dynamic Offset (Use an exponential curve)
        //float dynY = yOffset  + playerMovement.rb2DPlayerVelY * dynamYFactor;
        float velY = playerMovement.rb2DPlayerVelY;
        float dynY = yOffset + Mathf.Sign(velY) * Mathf.Pow(Mathf.Abs(velY),dynamYExpFactor) * dynamYFactor;
        dynY = Mathf.Clamp(dynY,-2f,+2f);

        //Update the tracking Position by changing the camera Target LocalPos (as is GO's child of the Player)
        //cameraTarget.localPosition = new Vector3(dynX,dynY,0f);
        UpdateOffsets(dynX,dynY,zOffset);
    }
    #endregion
    #region Boundaries
    private void SetInitialBoundaries()
    {
        SetTargetBoundaries(TotalBoundsArea);

        currentBoundsArea = TotalBoundsArea;
        lastBoundsArea = TotalBoundsArea;

        //// Detecta qué collider (área de límites) contiene la posición inicial del player
        //Collider2D hit = Physics2D.OverlapPoint(player.position, LayerMask.GetMask("CamTriggerArea"));

        //if (hit != null)
        //{
        //    //CamBoundariesTriggerArea area = hit.GetComponent<CamBoundariesTriggerArea>();
        //    CamBoundariesTriggerArea area = hit.GetComponent<CamBoundariesTriggerArea>();

        //    if (area != null)
        //        SetTargetBoundaries(area);            
        //}
        //else
        //{
        //    Debug.LogWarning("CameraFollow: Was not found a Cam Limited area on the player initial position.");            
        //}
    }
    public void SetTargetBoundaries(CamBoundariesTriggerArea enteringArea)
    {        
        if(confiner2D != null && enteringArea != null)
        {
            var boundsCollider = enteringArea.BoundsCollider;
            if (boundsCollider == null)
                Debug.LogError("The Bounds Collider Not Found on the gameobject " + enteringArea.gameObject);
            else
            {
                if (confiner2D.BoundingShape2D != null)
                    return;

                // Assign the Collider of the new entering area to the Cinemachine Confiner2
                confiner2D.BoundingShape2D = boundsCollider;
                // The Confiner will be updated on the next frame
                
                // Set the current Area as the last Bondary Area and the New are as the current one
                lastBoundsArea = currentBoundsArea;
                currentBoundsArea = enteringArea;
            }                
        }
    }
    public void ClearTargetBoundaries(CamBoundariesTriggerArea exitingArea)
    {
        // Remove the Confiner 2D boundaires "No limits", (only if we are exiting from a
        // different area than the assigned one)


        // Every time we leave an area we have 2 scenarios checkin the confiner2D.BoundingShape2D:
        // 1. It's set the same than the area we are currently exiting --> Set the boundaries of the Previous Saved Area
        // 2. It's set a different one than the area we are currently exiting --> Just Update the Previous Area Ref. to the "non-limits" one.        
        if(confiner2D != null)
        {            
            if (confiner2D.BoundingShape2D == exitingArea.BoundsCollider)
            {
                SetTargetBoundaries(lastBoundsArea);                
            }                             
            else if (confiner2D.BoundingShape2D != exitingArea.BoundsCollider)
            {
                lastBoundsArea = TotalBoundsArea;                
            }              
        }
    }
    #endregion
    #region Offsets
    private void UpdateOffsets(float dynX, float dynY, float zOffset)
    {
        if(composer != null)
        {
            composer.TargetOffset = new Vector3(dynX,dynY,zOffset);
        }
    }
    #endregion
    #region Camera Effects
    private void CameraShaking(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {        
        //Vector3 impulse = new Vector3(thrustEnemyDir.x, thrustEnemyDir.y, 0f) * thrustEnemyForce * impulseForceFactor;
        Vector3 impulse = new Vector3(thrustEnemyDir.x, thrustEnemyDir.y, 0f) * impulseForceFactor;
        impulseSource.GenerateImpulse(impulse);
    }
    //private void CameraShaking(Vector2 thrustEnemyDir, float thrustEnemyForce)
    //{
    //    Camera.main.transform.DOShakePosition(
    //        duration: shakeDuration,           // Duración total del shake
    //        strength: shakeStrength,         // Magnitud del movimiento (puede ser Vector3)
    //        vibrato: shakeVibrato,            // Cuántas veces vibra en ese tiempo
    //        randomness: 90f,        // Aleatoriedad del movimiento
    //        snapping: false,        // Si debe redondear los valores a enteros (tiles, píxeles, etc.)
    //        fadeOut: true           // Si el shake se va desvaneciendo hacia el final
    //);
    //}    
    #endregion
    #region References
    public void GetRefsOfPlayerMovement(PlayerMovement pMove)
    {
        // Get the Player Movement & Player Sprite Refs.
        playerMovement = pMove;
        playerSprite = pMove.SpriteRendPlayer;
        //playerMovement.OnPlayerAttackEnemy += CameraShaking;

        // Get the Player GO Ref.
        player = playerMovement.gameObject.transform;
        if (player == null)
            Debug.LogError("Player GO not found as GO of PlayerMovement!!");

        // Get the Camera Target Ref.
        cameraTarget = player.Find("CameraTarget");
        if (cameraTarget == null)
            Debug.LogError("Camera Target not found as a child of the Player GO!!");

    }
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnHitFXPlayer += CameraShaking;
    }
    #endregion
    #region Camera Enabling
    public void StopCameraFollow()
    {
        cameraFollowEnabled = false;
    }
    #endregion
}
