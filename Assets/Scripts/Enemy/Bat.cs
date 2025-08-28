using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static PlayerMovement;

public class Bat : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] patrolPoints;     // Points where I want my enemy to patrol    
    [SerializeField] Transform middlePatrolPoint;  // Middle Point of my patrol
    [SerializeField] private bool isReachedPos;    
    private struct Positions
    {
        public Vector2 targetPosition;
        public Vector2 originPosition;
        public Vector2 middlePoint;
        public Vector2 bezierControlPoint;
    }
    private Positions positions;              

    [Header("Damage")]
    [SerializeField] private int damageAmount;    

    // Movement vars
    //[Header("Attack And Movement")]
    //[SerializeField] float attackSpeed;               // Bat speed during Attack

    [Header("Raycast")]    
    [SerializeField] LayerMask playerLayer;         // Player Layer
    //[SerializeField] float rayLength;               //Raycast Length
    [SerializeField] float detectAreaDistance;      // Raycast Length for player's detection
    //[SerializeField] float attackAreaDistance;      // Raycast Length for attack's area
    [SerializeField] bool isOnDetectArea;           // Player is on detection Area flag
    [SerializeField] bool isOnAttackArea;           // Player is on Attack Area flag
    Vector2 enemyToPlayerVector;
    private bool wasDetecting;                      // Previous State of Player detection flag    

    [Header("Player")]
    private GameObject player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerSFX playerSFX;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f
                                                        // Velocity --> 25f    
    [Header("Sprite")]
    [SerializeField] bool xPosDirSpriteValue;    // needed value on SpriteRenderer.flipX 
                                                 // to get a sprite looking to Vector2.right dir.

    [Header("Appearance Timer")]
    /*[SerializeField]*/ private float appearanceMaxTime;
    [SerializeField] private float appearanceTimer;
    [SerializeField] private bool isAppearanceTimerEnabled;

    [Header("Return To Idle Timer")]
    /*[SerializeField]*/ private float returnToIdleMaxTime;
    [SerializeField] private float returnToIdleTimer;
    [SerializeField] private bool isReturnToIdleTimerEnabled;

    [Header("Attack")]    
    /*[SerializeField]*/ private float attackMaxTime;           // Calculated in func. of the Attack Anim Length
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isAttackTimerEnabled;    
    [SerializeField] private float waitForAttackMaxTime;
    [SerializeField] private float waitForAttackTimer;
    [SerializeField] private bool isWaitForAttackTimerEnabled;
    [SerializeField] private bool isFirstAttackDone;
    [SerializeField] private bool isAttackDone;    

    [Header("Death")]
    [SerializeField] private bool isDeath;

    [Header("SFX")]
    [SerializeField] private AudioClip idleSFX;
    [SerializeField] private AudioClip alertSFX;    
    [SerializeField] private AudioClip idleAlertSFX;            
    [SerializeField] private float delayIdleAlertSFX;
    [SerializeField] private AudioClip flyingAlertSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip deathSFX;

    public enum EnemyState
    {
        Idle,
        Appearance,
        IdleAlert,        
        AttackAndMovement,
        ReturnToIdle,
        Death        
    }
    [Header("Enums")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    public EnemyState CurrentState => currentState;

    // Boolean Flags
    private bool isPlayerDetectionEnabled;    

    // GOs 
    SpriteRenderer spriteRenderer;
    Animator animator;
    //Animator scratchAnimator;
    //SpriteRenderer scratchSprite;
    AudioSource audioSource;    
    Collider2D hurtBoxCollider;                         // Trigger collider used for detecting player attacks (ReceivePlayerAttack)
    Collider2D hitBoxCollider;                          // Trigger collider used as the attack collider (AttackPlayer)    
    Collider2D attackAreaCollider;                      // Trigger collider used as the attack area collider (AttackPlayer)    

    private Vector2 attackAreaColliderWorldPos = Vector2.zero;    

    #region Unity API
    void Awake()
    {
        // Get the Component Refs. from this GO
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("The AudioSource Component does not exist in this GO.");
        else        
            PlayIdleSFX();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("The Sprite Renderer Component does not exist in this GO.");

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("The Animator Component does not exist in this GO.");        

        // Get the Components refs. from the children
        hitBoxCollider = transform.Find("HitBox").GetComponent<Collider2D>();
        if (hitBoxCollider == null)
            Debug.LogError("The HitBox Collider Component was not found on any child of the gameobject " + gameObject);        

        hurtBoxCollider = transform.Find("HurtBox").GetComponent<Collider2D>();
        if (hurtBoxCollider == null)
            Debug.LogError("The HurtBox Collider Component was not found on any child of the gameobject " + gameObject);        

        attackAreaCollider = transform.Find("AttackArea").GetComponent<Collider2D>();
        if (attackAreaCollider == null)
            Debug.LogError("The Attack Area Collider Component was not found on any child of the gameobject " + gameObject);
        else
        {
            // Save the initial World Pos. of the Attack Area Collider
            attackAreaColliderWorldPos = attackAreaCollider.transform.position;
        }

        // Get the Player GO's References
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("The Enemy can't find the GO Player");
        else
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement == null)
                Debug.LogError("The component PlayerMovement was not found on the Player GO");
            else
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                    Debug.LogError("The component PlayerHealth was not found on the Player GO");
                else
                {
                    playerSFX = player.GetComponent<PlayerSFX>();
                    if (playerSFX == null)
                        Debug.LogError("The component PlayerSFX was not found on the Player GO");
                }
            }
        }

        // Set the Target Position Array and the Attack Speed
        if (patrolPoints == null)
            Debug.LogError("The Patrol Points GO does not contain any position references.");
        else
        {
            // Set the Patrol positions (origin and target) & the initial position                                    
            positions.originPosition = patrolPoints[0].position;
            positions.targetPosition = patrolPoints[1].position;
            positions.middlePoint = middlePatrolPoint.position;

            transform.position = positions.originPosition;

            // Get the Attack and Apperance Animation Lengths in secs.
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            foreach (var clip in ac.animationClips)
            {
                if (clip.name == "Attack")
                    attackMaxTime = clip.length;
                else if (clip.name == "Appearance")
                    appearanceMaxTime = clip.length;
                else if (clip.name == "ReturnToIdle")
                    returnToIdleMaxTime = clip.length;
            }

            // Calculate the Attack Speed in func. of the Attack Animation Length
            //attackSpeed = Mathf.Abs(positions.originPosition.x - positions.targetPosition.x) /attackMaxTime;
        }        
    }    
    private void Update()
    {                        

    }
    private void FixedUpdate()
    {
        // Get Player To Enemy Dir. Vector and the Sprite-X Direction
        GetEnemyToPlayerDir();

        // Detect Player on DetectArea (Idle -> Appearance -> IdleAlert)        
        CheckDetectionArea();

        // Detect Player on AttackArea (IdleAlert -> Attack)
        // Managed through 'AttackArea' BoxCollider2D.

        // Patrol (Circle Arc from current->target A->B or B->A)
        
        // Update the Enemy state
        UpdateEnemyState();

        Debug.Log(attackAreaCollider.enabled);
    }

    public void OnChildTriggerEnter(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player") && isPlayerDetectionEnabled)
        {
            Debug.Log("Player entered on the HitBox");

            DisablePlayerDetection();

            Attack();            
        }
        else if (id == "HurtBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player entered on the HurtBox");

            ReceivePlayerAttack();
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = true;
            Debug.Log("Player entered on the AttackArea");
        }
    }
    public void OnChildTriggerExit(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player exit from the HitBox");
        }
        else if (id == "HurtBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player exit from the HurtBox");
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = false;
            Debug.Log("Player exit from the AttackArea");
        }
    }
    #endregion

    #region Enemy State
    // Enemy State
    private void UpdateEnemyState()
    {
        if (isDeath && currentState != EnemyState.Death)
        {
            // Play the SFX
            StopAudioSource();
            PlayDeathSFX();            

            // Setup the GO destruction
            Destroy(gameObject, 3f);

            // Debug
            Debug.Log("From " + currentState + " state to Death State. Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");

            // State Update
            currentState = EnemyState.Death;

            // Anims update
            UpdateAnimations();
        }
        else
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    // State Update
                    if (isOnDetectArea)
                    {                  
                        // Set the Appearance Timer
                        SetAppearanceTimer();

                        // Update the state
                        currentState = EnemyState.Appearance;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Appearance:
                    // Idle Timer Update
                    UdpateAppearanceTimer();

                    if (!isAppearanceTimerEnabled)
                    {
                        // Enable the Player Detection again
                        EnablePlayerDetection();

                        // Enable the Attack Area Collider again
                        EnableAttackAreaCollider(true);

                        // Update the state
                        currentState = EnemyState.IdleAlert;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Appearance state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.IdleAlert:
                    // Attack Timer Update (Only when at least 1 attack has been performed)
                    if (isFirstAttackDone && isWaitForAttackTimerEnabled)
                        UdpateWaitForAttackTimer();

                    // State Update
                    if (!isOnDetectArea)
                    {
                        // Set the Appearance Timer
                        SetReturnToIdleTimer();

                        // Reset the Attack Timer and the First Attack Done flag
                        ResetWaitForAttackTimer();
                        ResetFirstAttackDone();

                        // Disable the Attack Area Collider till we came back again to Idle Alert State
                        EnableAttackAreaCollider(false);

                        // Update the state
                        currentState = EnemyState.ReturnToIdle;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Idle Alert state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (isOnAttackArea && !isWaitForAttackTimerEnabled)
                    {
                        // Set the Patrol Timer
                        SetAttackTimer();
                        // Set the corresponding Target pos. of the Arc Patrol
                        SetNextTargetPosition();

                        // Disable the Attack Area Collider during the Attack Animation
                        EnableAttackAreaCollider(false);

                        // Get the Bezier Control Point (needed for reaching the set middle point in the half of the attack anim.)
                        positions.bezierControlPoint = GetBezierMiddleTargetPoint(
                                                positions.originPosition,
                                                positions.targetPosition,
                                                positions.middlePoint);

                        // Update the state
                        currentState = EnemyState.AttackAndMovement;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Idle Alert state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.AttackAndMovement:
                    // Idle Timer Update
                    UdpateAttackTimer();

                    // Patrol Update
                    UpdateTargetPosition(0.01f);
                    ArcPatrol(Mathf.Clamp01(1 - (attackTimer/attackMaxTime)), isAttackTimerEnabled);

                    //if (isAttackDone)
                    if(!isAttackTimerEnabled /*&& isReachedPos*/)
                    {
                        // Reset the Attack Done Flags                        
                        if (!isFirstAttackDone)
                            SetFirstAttackDone();

                        // Set the Attack Timer
                        SetWaitForAttackTimer();

                        // Enable the Player Detection again
                        EnablePlayerDetection();

                        // Enable the Attack Area Collider again
                        EnableAttackAreaCollider(true);

                        // Update the state
                        currentState = EnemyState.IdleAlert;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Attack & Movement state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.ReturnToIdle:
                    // Return To Idle Timer Update
                    UdpateReturnToIdleTimer();

                    if (!isReturnToIdleTimerEnabled)
                    {
                        // Update the state
                        currentState = EnemyState.Idle;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        Debug.Log("From Return To Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Death:
                    Debug.Log("I am on Death State");
                    break;
                default:
                    // Default logic
                    break;
            }
        }
    }
    #endregion
    #region Return To Idle Timer    
    private void UdpateReturnToIdleTimer()
    {
        // Idle Timer update
        returnToIdleTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (returnToIdleTimer <= 0)
        {
            ResetReturnToIdleTimer();
        }
    }
    private void ResetReturnToIdleTimer()
    {
        isReturnToIdleTimerEnabled = false;
        returnToIdleTimer = 0f;
    }
    private void SetReturnToIdleTimer()
    {
        returnToIdleTimer = returnToIdleMaxTime;
        isReturnToIdleTimerEnabled = true;
    }
    #endregion
    #region Appearance Timer    
    private void UdpateAppearanceTimer()
    {
        // Idle Timer update
        appearanceTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (appearanceTimer <= 0)
        {
            ResetAppearanceTimer();
        }
    }
    private void ResetAppearanceTimer()
    {
        isAppearanceTimerEnabled = false;
        appearanceTimer = 0f;
    }
    private void SetAppearanceTimer()
    {
        appearanceTimer = appearanceMaxTime;
        isAppearanceTimerEnabled = true;
    }
    #endregion
    #region Wait for Attack Timer       
    private void UdpateWaitForAttackTimer()
    {
        // Idle Timer update
        waitForAttackTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (waitForAttackTimer <= 0)
        {
            ResetWaitForAttackTimer();
        }
    }
    private void ResetWaitForAttackTimer()
    {
        isWaitForAttackTimerEnabled = false;
        waitForAttackTimer = 0f;
    }
    private void SetWaitForAttackTimer()
    {
        waitForAttackTimer = waitForAttackMaxTime;
        isWaitForAttackTimerEnabled = true;
    }
    #endregion

    #region Player Detection
    private void GetEnemyToPlayerDir()
    {
        // Update Enemy To Player Vector
        enemyToPlayerVector = (player.transform.position - transform.position);

        // Update Sprite X-Flip       
        FlipSprite();
    }
    private void CheckDetectionArea()
    {        
        // Detection Area flag update
        isOnDetectArea = (Vector2.Distance(transform.position, player.transform.position) <= detectAreaDistance);

        // Raycast Debugging
        //Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
        Debug.DrawRay(transform.position, enemyToPlayerVector.normalized * detectAreaDistance, Color.red);
    }
    void EnablePlayerDetection()
    {
        isPlayerDetectionEnabled = true;
    }
    public void DisablePlayerDetection()
    {
        isPlayerDetectionEnabled = false;
    }
    #endregion

    #region Colliders
    private void EnableAttackAreaCollider(bool enable)
    {
        if (enable)
            SetAttackAreaColliderPos();
        attackAreaCollider.enabled = enable;
    }
    private void SetAttackAreaColliderPos()
    {
        // Set the correct offset for the Attack Collider 
        attackAreaCollider.transform.position = attackAreaColliderWorldPos;
    }
    #endregion

    #region Attack
    #region Enemy Attack
    public void Attack()
    {
        // Get the Enemy's direction
        Vector2 enemyDirection = spriteRenderer.flipX ?
                                Vector2.left + Vector2.up :
                                Vector2.right + Vector2.up;

        // Take Player's Damage & Disable the player's detection for a certain time            
        playerHealth.TakeDamage(damageAmount, enemyDirection, thrustToPlayer);
    }
    #endregion
    #region Player Attack
    private void ReceivePlayerAttack()
    {
        if (playerMovement.IsGrounded)
            return;

        playerMovement.UpwardsEnemyImpulse();
        playerSFX.PlayEnemyJumpSFX();

        isDeath = true;
    }
    #endregion
    #region Attack Patrol
    void UpdateTargetPosition(float threshold)
    {        
        // Check if the enemy reached its target position
        if (Vector2.Distance(transform.position, positions.targetPosition) < threshold)
            SetReachedPosFlag(true);
        else
            SetReachedPosFlag(false);
    }
    void SetNextTargetPosition()
    {
        if (transform.position == patrolPoints[0].position)
        {
            positions.originPosition = patrolPoints[0].position;
            positions.targetPosition = patrolPoints[1].position;
        }
        else if (transform.position == patrolPoints[1].position)
        {
            positions.originPosition = patrolPoints[1].position;
            positions.targetPosition = patrolPoints[0].position;
        }
        else
            Debug.LogError("The current Transform position is not any of the defined ones");

        SetReachedPosFlag(false);
    }
    private void SetReachedPosFlag(bool enable)
    {
        isReachedPos = enable;
    }
    //void Patrol(float speed)
    //{
    //    // Update the ant's position
    //    Vector2 newPos = Vector3.MoveTowards(transform.position, positions.targetPosition, speed * Time.fixedDeltaTime);
    //    transform.position = newPos;
    //}
    void ArcPatrol(float time, bool isTimerEnabled)
    {
        if (isTimerEnabled)        
            transform.position = QuadraticBezier(positions.originPosition, positions.targetPosition, positions.bezierControlPoint, time);                    
        else        
            transform.position = positions.targetPosition;                                     
    }
    private Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        return Mathf.Pow(1 - t, 2) * a + 2 * (1 - t) * t * c + Mathf.Pow(t, 2) * b;
    }
    private Vector2 GetBezierMiddleTargetPoint(Vector2 a, Vector2 b, Vector2 c)
    {
        return 2f * c - 0.5f * (a + b);
    }
    #endregion
    #region Attack Timer       
    private void UdpateAttackTimer()
    {
        // Idle Timer update
        attackTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (attackTimer <= 0)
        {
            ResetAttackTimer();
        }
    }
    private void ResetAttackTimer()
    {
        isAttackTimerEnabled = false;
        attackTimer = 0f;
    }
    private void SetAttackTimer()
    {
        attackTimer = attackMaxTime;
        isAttackTimerEnabled = true;
    }
    #endregion
    #region Attack Flags    
    public void SetFirstAttackDone()
    {
        isFirstAttackDone = true;
    }
    public void ResetFirstAttackDone()
    {
        isFirstAttackDone = false;
    }
    #endregion
    #endregion

    #region Sprite & Animations
    void FlipSprite()
    {         
        spriteRenderer.flipX = (enemyToPlayerVector.x >= 0) ? false : true;        
    }
    private void ClearAnimationFlags()
    {
        animator.ResetTrigger("IsDying");
        animator.ResetTrigger("Attack");        
    }
    private void UpdateAnimations()
    {
        ClearAnimationFlags();

        switch (currentState)
        {
            case EnemyState.Idle:
                //animator.SetTrigger("IsIdle");
                StopAudioSource();
                PlayIdleSFX();
                break;
            case EnemyState.Appearance:
                StopAudioSource();
                PlayAlertSFX();
                animator.SetBool("IsPlayerDetected",isOnDetectArea);
                break;
            case EnemyState.IdleAlert:
                StopAudioSource();
                PlayFlyingAlertSFX();
                StartCoroutine(nameof(PlayIdleAlertContinuously));
                break;
            case EnemyState.AttackAndMovement:
                StopAudioSource();
                PlayAttackSFX();
                animator.SetTrigger("Attack");
                break;
            case EnemyState.ReturnToIdle:
                StopAudioSource();
                //PlayReturnToIdleSFX();
                animator.SetBool("IsPlayerDetected", isOnDetectArea);
                break;
            case EnemyState.Death:
                animator.SetTrigger("IsDying");
                break;
            default:
                break;
        }
    }
    #endregion

    #region Audio
    public void PlayOneShotSFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    public void PlayOnLoopSFX(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
    public void StopAudioSource()
    {
        audioSource.Stop();
    }
    private void PlayIdleSFX()
    {
        PlayOnLoopSFX(idleSFX);
    }
    public void PlayAlertSFX()
    {
        PlayOneShotSFX(alertSFX);
    }
    private IEnumerator PlayIdleAlertContinuously()
    {        
        while (currentState == EnemyState.IdleAlert)
        {            
            PlayIdleAlertSFX();
            yield return new WaitForSeconds(idleAlertSFX.length + delayIdleAlertSFX);
        }                
    }
    private void PlayIdleAlertSFX()
    {
        PlayOneShotSFX(idleAlertSFX);
    }    
    private void PlayFlyingAlertSFX()
    {
        PlayOnLoopSFX(flyingAlertSFX);
    }
    public void PlayAttackSFX()
    {
        PlayOneShotSFX(attackSFX);
    }    
    private void PlayDeathSFX()
    {
        PlayOneShotSFX(deathSFX);
    }
    #endregion
}
