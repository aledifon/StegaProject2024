using DG.Tweening;
using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
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
    [SerializeField,Range(0f,50f)] private float impulseForceHitFactor;         //1f       
    [SerializeField, Range(0.05f, 5f)] private float impulseHitDuration;        //0.3f (for CameraShaking)       
    [SerializeField, Range(0.1f, 10f)] private float impulseHitFrequencyGain;   //8f       
    [SerializeField, Range(0.1f, 10f)] private float impulseHitAmplitudeGain;   //0.25       

    [SerializeField,Range(0f,50f)] private float impulseForceDeathFactor;       //0.2f           
    [SerializeField, Range(0.05f, 5f)] private float impulseDeathDuration;      //1f (for CameraDeathShaking)           
    [SerializeField,Range(0.1f,100f)] private float impulseDeathFrequencyGain;  //8f       
    [SerializeField,Range(0.05f,10f)] private float impulseDeathAmplitudeGain;   //0.1f       

    [Header("Boundaries")]
    [SerializeField] CamBoundariesTriggerArea TotalBoundsArea;
    private CinemachineConfiner2D confiner2D;
    [SerializeField] CamBoundariesTriggerArea currentBoundsArea;
    [SerializeField] CamBoundariesTriggerArea lastBoundsArea;

    private bool ignoreTriggerAtStart = true;
    private bool confinerTriggersEnabled = false;
    public bool ConfinerTriggersEnabled => confinerTriggersEnabled; 
    private float ignoreTriggerTime = 0.2f;    

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
        if (GameManager.Instance.IsDebuggingMode)
            SetTargetBoundaries(CheckCamTriggerArea());
        else if (player != null && GameManager.Instance.LastCheckpointData != null)
            SetConfinerFromCheckpoint(GameManager.Instance.LastCheckpointData.respawnCamBoundTriggerArea);        

        // Manage when the Collider Triggers of the Confiners will be enabled
        if (ignoreTriggerAtStart)
            StartCoroutine(EnableTriggersAfterDelay(ignoreTriggerTime));
        else
            confinerTriggersEnabled = true;
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
    private IEnumerator EnableTriggersAfterDelay(float ignoreTime)
    {
        yield return new WaitForSeconds(ignoreTime);
        confinerTriggersEnabled = true;
    }
    public void SetConfinerFromCheckpoint(CamBoundariesTriggerArea camArea)
    {               
        SetTargetBoundaries(camArea);

        currentBoundsArea = camArea;
        lastBoundsArea = null;        
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
                if (confiner2D.BoundingShape2D == enteringArea.BoundsCollider)
                    return; // ya estamos usando este confiner

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
                SetTargetBoundaries(CheckCamTriggerArea());
            }                             
            else if (confiner2D.BoundingShape2D != exitingArea.BoundsCollider)
            {
                lastBoundsArea = null;                
            }              
        }
    }
    private CamBoundariesTriggerArea CheckCamTriggerArea()
    {
        CamBoundariesTriggerArea area;
        area = null;

        // Detecta qué collider (área de límites) contiene la posición inicial del player
        Collider2D hit = Physics2D.OverlapPoint(player.position, LayerMask.GetMask("CamTriggerArea"));        

        if (hit != null)                    
            area = hit.GetComponent<CamBoundariesTriggerArea>();

        if (area == null)
            area = TotalBoundsArea;

        return area;
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
        Vector3 impulse = new Vector3(thrustEnemyDir.x, thrustEnemyDir.y, 0f) * impulseForceHitFactor;

        // Set the oscilations settings        
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;

        impulseSource.ImpulseDefinition.ImpulseDuration = impulseHitDuration;
        impulseSource.ImpulseDefinition.FrequencyGain = impulseHitFrequencyGain;
        impulseSource.ImpulseDefinition.AmplitudeGain = impulseHitAmplitudeGain;

        impulseSource.GenerateImpulse(impulse);
    }
    public void CameraDeathShaking()
    {
        //Vector3 impulse = new Vector3(thrustEnemyDir.x, thrustEnemyDir.y, 0f) * thrustEnemyForce * impulseForceFactor;
        Vector3 direction = new Vector3(1f,1f,0f);
        Vector3 impulse = direction * impulseForceDeathFactor;

        // Set the oscilations settings
        // Set the oscilations settings        
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;

        impulseSource.ImpulseDefinition.ImpulseDuration = impulseDeathDuration;
        impulseSource.ImpulseDefinition.FrequencyGain = impulseDeathFrequencyGain;
        impulseSource.ImpulseDefinition.AmplitudeGain = impulseDeathAmplitudeGain;

        impulseSource.GenerateImpulse(impulse);

        StartCoroutine(nameof(TriggerDeathPanel));
    }
    private IEnumerator TriggerDeathPanel()
    {
        yield return new WaitForSeconds(impulseDeathDuration);
        GameManager.Instance.ChooseDeathOrGameOverPanel();
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
        //playerHealth.OnDeathPlayer += CameraDeathShaking;
    }
    public void UnsubscribeEventsOfPlayerHealth()
    {        
        playerHealth.OnHitFXPlayer -= CameraShaking;
        //playerHealth.OnDeathPlayer -= CameraDeathShaking;
        playerHealth = null;
    }
    #endregion
    #region Camera Enabling
    public void StopCameraFollow()
    {
        cameraFollowEnabled = false;
    }
    #endregion
}
