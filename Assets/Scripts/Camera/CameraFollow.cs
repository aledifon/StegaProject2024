using DG.Tweening;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;        

    // Player's Info
    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    SpriteRenderer playerSprite;
    int playerDir = 1;
    Vector3 smoothDampVelocity;            // Player's current speed storage (Velocity Movement type through r            

    [Header("Camera positioning")]
    [SerializeField] float defCamSize;          // Default Camera Size (Zoom) || 6.5f    
    float sizeVelocity;

    [SerializeField] float MinSmoothCamTime;
    [SerializeField] float MaxSmoothCamTime;        
    [SerializeField] float camOffsetLerpSpeed;  // Speed of Camera Offset Lerp  || 5f
    Vector3 offset;                             // Camera Offset (Distance between the camera and the player)
    [SerializeField] float defXCamOffset;       // Default Camera Offset on X-axis      || 3f
    float xCamOffset;                           // Current Offset on X-axis
    float dynamXCamOffset;                      // Dynamic X-Offset and factor to anticipate running
    [SerializeField] float dynamXCamOffsetFactor;   // 0.05f-0.1f

    [SerializeField] float defYCamOffset;       // Default Camera Offset on Y-axis      || 0f
    float yCamOffset;                           // Current  Camera Offset on Y-axis
    float dynamYCamOffset;                      // Dynamic Y-Offset and factor to anticipate jumps and falls
    [SerializeField] float dynamYCamOffsetFactor;   // 0.05f-0.1f

    [SerializeField] float defZCamOffset;       // Camera Offset on Z-axis      || -12f                                           
    float zCamOffset;                           // Camera Offset on Z-axis                                         

    Vector3 smoothOffsetVelocity;

    private bool cameraFollowEnabled = true;

    float dynamicSmoothCamTime;

    // Camera Boundaries
    [Header ("Boundaries")]
    [SerializeField] Vector4 currentBounds;
    [SerializeField] Vector4 targetBounds;
    [SerializeField] float boundsLerpSpeed = 3f;
    [SerializeField] CamBoundariesTriggerArea currentBoundsArea;
    [SerializeField] CamBoundariesTriggerArea lastBoundsArea;

    [Header("Camera Shaking")]
    [SerializeField] float shakeDuration;   // 2f
    [SerializeField] float shakeStrength;   // 0.5f
    [SerializeField] int shakeVibrato;    // 100
    [SerializeField] float shakeRandomness; // 90f


    void Awake()
    {
        //offset = transform.position - player.position;  // Calculate the initial distance between the camera and the player

        // Set the Camera Default Offsets & the Camera Def. Size
        SetCameraDefSettings(1f);
        offset = new Vector3(xCamOffset,yCamOffset,zCamOffset);

        // Set the max Smooth Time as the default time the camera wil move (This means a slower follow up of the player)
        dynamicSmoothCamTime = MaxSmoothCamTime;
    }
    private void Start()
    {
        // Set the initial Cam Boundaries
        SetInitialBoundaries();
    }
    //void FixedUpdate()
    //{        
    //    //// Do nothing till we get all the player's refs.
    //    //if (playerSprite == null || playerMovement == null)
    //    //    return;

    //    //// Get the Player's view direction
    //    //playerDir = playerSprite.flipX ? -1 : 1;

    //    //// Set the Camera Offset in func. of the player's direction
    //    //dynamYCamOffset = yCamOffset;
    //    //dynamYCamOffset += playerMovement.rb2DPlayerVelY * dynamYCamOffsetFactor;
    //    //Vector3 targetOffset = new Vector3(xCamOffset * playerDir, dynamYCamOffset, zCamOffset);

    //    //// Optional Lerp to smooth the offset changes
    //    //offset = Vector3.Lerp(offset, targetOffset, Time.fixedDeltaTime * camOffsetLerpSpeed);                        
    //}
    private void LateUpdate()
    {
        if (!cameraFollowEnabled & (playerSprite == null || playerMovement == null))
            return;

        // Get the Player's view direction
        playerDir = playerSprite.flipX ? -1 : 1;

        // Set the Camera Offset in func. of the player's direction
        // A dynamic offset is used for the Y-axis in func. of the player's vert. speed
        dynamYCamOffset = yCamOffset;
        dynamYCamOffset += playerMovement.rb2DPlayerVelY * dynamYCamOffsetFactor;
        // A dynamic offset is used for the X-axis in func. of the player's vert. speed
        //dynamXCamOffset = xCamOffset;
        float baseOffsetX = xCamOffset * playerDir;
        float velocityOffsetX = playerMovement.rb2DPlayerVelX * dynamXCamOffsetFactor;
        dynamXCamOffset = baseOffsetX + velocityOffsetX;
        Vector3 targetOffset = new Vector3(dynamXCamOffset, dynamYCamOffset, zCamOffset);

        // Lerp applied to smooth the offset changes
        //offset = Vector3.Lerp(offset, targetOffset, Time.fixedDeltaTime * camOffsetLerpSpeed);        
        offset = Vector3.SmoothDamp(offset, targetOffset, ref smoothOffsetVelocity ,0.1f);

        // Get the Camera Bounds (Get the half of the height and the half of the width of the Camera)
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        // Smooth the change of boundaries between areas
        currentBounds = Vector4.Lerp(currentBounds, targetBounds, Time.deltaTime * boundsLerpSpeed);

        // Dynamic SmoothCamtime in func. of the player's vertical speed --> Higher speeds then value close to min = faster follow-up
                                                                        //--> Slower speeds then value close to max = slowerr follow-up
        float playerVertSpeed = Mathf.Abs(playerMovement.rb2DPlayerVelY);
        float targetSmoothCamTime = Mathf.Lerp(
                                    MinSmoothCamTime,
                                    MaxSmoothCamTime,
                                    Mathf.Clamp01(1 - (playerVertSpeed * 0.05f / 0.3f)));

        dynamicSmoothCamTime = Mathf.Lerp(dynamicSmoothCamTime, targetSmoothCamTime, Time.fixedDeltaTime * camOffsetLerpSpeed);

        // Set the target position
        Vector3 targetPos = player.position + offset;
        // Clamp the target position between the camera bounds previously calculated.
        targetPos.x = Mathf.Clamp(targetPos.x, currentBounds.x + camHalfWidth, currentBounds.y - camHalfWidth);
        targetPos.y = Mathf.Clamp(targetPos.y, currentBounds.z + camHalfHeight, currentBounds.w - camHalfHeight);

        // Apply the SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, 
                                            ref smoothDampVelocity, dynamicSmoothCamTime);        
    }
    public void StopCameraFollow()
    {
        cameraFollowEnabled = false;
    }
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnHitFXPlayer += CameraShaking;
    }
    public void GetRefsOfPlayerMovement(PlayerMovement pMove)
    {
        playerMovement = pMove;
        playerSprite = pMove.SpriteRendPlayer;
        //playerMovement.OnPlayerAttackEnemy += CameraShaking;
    }
    private void CameraShaking(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        Camera.main.transform.DOShakePosition(
            duration: shakeDuration,           // Duración total del shake
            strength: shakeStrength,         // Magnitud del movimiento (puede ser Vector3)
            vibrato: shakeVibrato,            // Cuántas veces vibra en ese tiempo
            randomness: 90f,        // Aleatoriedad del movimiento
            snapping: false,        // Si debe redondear los valores a enteros (tiles, píxeles, etc.)
            fadeOut: true           // Si el shake se va desvaneciendo hacia el final
    );
    }

    #region Camera Settings Methods    
    public void SetCameraSettings(CamOffsetMaskEnum.CamOffsetMask camOffsetMask, 
                                float xNewCamOffset, 
                                float yNewCamOffset, 
                                float zNewCamOffset,
                                float sizeNewCam, float sizeSmoothTime)
    {
        // Update the corresponding Axis
        if ((camOffsetMask & CamOffsetMaskEnum.CamOffsetMask.X) != 0)
            xCamOffset = defXCamOffset;

        if ((camOffsetMask & CamOffsetMaskEnum.CamOffsetMask.Y) != 0)
            yCamOffset = defYCamOffset;

        if ((camOffsetMask & CamOffsetMaskEnum.CamOffsetMask.Z) != 0)
            zCamOffset = defZCamOffset;

        if ((camOffsetMask & CamOffsetMaskEnum.CamOffsetMask.Size) != 0)
            StartCoroutine(SetCameraDefSize(sizeNewCam, sizeSmoothTime));
    }
    public void SetCameraDefSettings(float sizeSmoothTime)
    {
        // Set Camera Offsets
        xCamOffset = defXCamOffset;
        yCamOffset = defYCamOffset;
        zCamOffset = defZCamOffset;

        // Set Camera Size (Zoom)        
        if (Camera.main.orthographicSize != defCamSize)
            StartCoroutine(SetCameraDefSize(defCamSize, sizeSmoothTime));
    }
    public IEnumerator SetCameraDefSize(float targetCamSize, /*float speed*/ float smoothTime)
    {        
        while (Mathf.Abs(Camera.main.orthographicSize - targetCamSize) > 0.01f ) 
        {            
            // Lerp
            //Camera.main.orthographicSize = Mathf.Lerp(
            //    Camera.main.orthographicSize, 
            //    targetCamSize, 
            //    Time.deltaTime * speed);

            // SmoothDamp
            Camera.main.orthographicSize = Mathf.SmoothDamp(
                Camera.main.orthographicSize,
                targetCamSize,
                ref sizeVelocity,
                smoothTime);

            yield return null;
        }
        Camera.main.orthographicSize = targetCamSize;
    }
    #endregion

    #region Camera Boundaries
    private void SetInitialBoundaries()
    {
        // Detecta qué collider (área de límites) contiene la posición inicial del player
        Collider2D hit = Physics2D.OverlapPoint(player.position, LayerMask.GetMask("CamTriggerArea"));

        if (hit != null)
        {
            //CamBoundariesTriggerArea area = hit.GetComponent<CamBoundariesTriggerArea>();
            CamBoundariesTriggerArea area = hit.GetComponent<CamBoundariesTriggerArea>();

            if (area != null)
            {
                currentBounds = area.GetBounds();
                targetBounds = currentBounds;
            }
            // If the player is on non-Boundary area then "infinite" margins will be applied
            else
            {
                currentBoundsArea = null;
                currentBounds = new Vector4(-1000f, 1000f, -1000f, 1000f);
                targetBounds = currentBounds;
            }
                
        }
        else
        {
            Debug.LogWarning("CameraFollow: No se encontró un área de límites en la posición inicial del player.");
            currentBoundsArea = null;
            currentBounds = new Vector4(-1000f, 1000f, -1000f, 1000f);
            targetBounds = currentBounds;
        }
    }
    public void SetTargetBoundaries(CamBoundariesTriggerArea enteringArea)
    {
        // Set the current Area as the last Bondary Area and the New are as the current one
        lastBoundsArea = currentBoundsArea;
        currentBoundsArea = enteringArea;

        targetBounds = enteringArea.GetBounds();
    }
    public void ClearTargetBoundaries(CamBoundariesTriggerArea exitingArea)
    {
        // We exit from the new Area before exiting from the last One
        // and get their corresponding Boundaries
        if(exitingArea == currentBoundsArea)
        {
            // If there was a previous Bounds Area where we come from then we get their bounds
            if (lastBoundsArea != null)
            {
                currentBoundsArea = lastBoundsArea;
                targetBounds = currentBoundsArea.GetBounds();
                lastBoundsArea = null;
            }
            // Otherwise, we set "infinite bounds" in order to disable the Camera Boundaries
            else
            {
                currentBoundsArea = null;
                currentBounds = new Vector4(-1000f, 1000f, -1000f, 1000f);
                targetBounds = currentBounds;
            }
        }
        // We exit from the Last Area and we are still in a "bounding" Area
        else if (exitingArea == lastBoundsArea && currentBoundsArea != null)
        {
            lastBoundsArea = null;
        }
    }
    #endregion
}
