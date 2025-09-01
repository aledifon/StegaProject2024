using Demo_Project;
using UnityEngine;

public class Spider : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] patrolPoints;     // Points where I want my enemy to patrol        
    [SerializeField] private bool isReachedPos;    
    private struct Positions
    {
        public Vector2 targetPosition;
        public Vector2 originPosition;        
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

    [Header("Player")]
    private GameObject player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerSFX playerSFX;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f
                                                        // Velocity --> 25f        

    [Header("Idle Timer")]
    [SerializeField] private float idleMaxTime;
    [SerializeField] private float idleTimer;
    [SerializeField] private bool isIdleTimerEnabled;

    [Header("Attack Timer")]    
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isAttackTimerEnabled;
    private float attackMaxTime;                                // Calculated in func. of the Attack Anim Length    
    private float attackAnimLength;

    [Header("Cadence Attack Timer")]
    [SerializeField] private float cadenceAttackMaxTime;        // A Random value from min to max will be set in every frame
    [SerializeField] private float cadenceAttackMinTime;        
    [SerializeField] private float cadenceAttackTimer;
    [SerializeField] private bool isCadenceAttackTimerEnabled;    

    [Header("Speed")]
    [SerializeField] private float normalPatrolSpeed;
    private const float DefaultAnimSpeed = 1f;
    [SerializeField] private float alertPatrolSpeed;
    [SerializeField] private float alertPatrolAnimSpeed;

    [Header("Death")]
    [SerializeField] private bool isDeath;

    [Header("Projectiles")]
    [SerializeField] private GameObject projectilePrefab;
    private SpiderProjectile[] projectiles;
    [SerializeField,Range(2,8)] private int projectileNum;
    [SerializeField] private float projectilesInitAngle;            // = 45º;  (Angle applied to position the 1st Projectile)
    [SerializeField] private float projectileAngleDelta;            // Random angle delta applied every shoot
    private float projectilesAngleOffset;                           // = 360º/projectileNum; (Angle between projectiles)

    [Header("SFX")]
    [SerializeField] private AudioClip idleSFX;
    [SerializeField] private AudioClip normalPatrolSFX;
    [SerializeField] private AudioClip alertPatrolSFX;
    [SerializeField] private AudioClip alertSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip deathSFX;           

    public enum EnemyState
    {
        Idle,
        NormalPatrol,
        Attack,
        AlertPatrol,        
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
        GetGORefs();

        SetInitPatrol();

        projectiles = new SpiderProjectile[projectileNum];

        if (projectilePrefab != null)
            InitProjectiles();
        else
            Debug.LogError("No Prefab ref. for the projectiles is attached to this GO");

        // Player detection will be always enabled except for a few secs. when the player is detected.
        EnablePlayerDetection();
    }    
    private void Update()
    {                        

    }
    private void FixedUpdate()
    {
        // Get Player To Enemy Dir. Vector and the Sprite-X Direction
        //GetEnemyToPlayerDir();

        //// Detect Player on DetectArea (Not used for Spider Enemy)        
        //CheckDetectionArea();

        // Detect Player on AttackArea (Normal Patrol -> Alert Patrol)
        // Managed through 'AttackArea' CircleCollider2D.

        // Patrol (Vert. Movement from current->target A->B or B->A)
        
        // Update the Enemy state
        UpdateEnemyState();        
    }

    public void OnChildTriggerEnter(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player") && isPlayerDetectionEnabled)
        {
            //Debug.Log("Player entered on the HitBox");

            DisablePlayerDetection();

            Invoke(nameof(EnablePlayerDetection),1f);

            Attack();            
        }
        else if (id == "HurtBox" && collision.CompareTag("Player") && isPlayerDetectionEnabled)
        {
            //Debug.Log("Player entered on the HurtBox");

            DisablePlayerDetection();

            ReceivePlayerAttack();
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = true;
            //Debug.Log("Player entered on the AttackArea");
        }
    }
    public void OnChildTriggerExit(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player"))
        {
            //Debug.Log("Player exit from the HitBox");
        }
        else if (id == "HurtBox" && collision.CompareTag("Player"))
        {
            //Debug.Log("Player exit from the HurtBox");
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = false;
            //Debug.Log("Player exit from the AttackArea");
        }
    }
    #endregion

    #region GO Refs
    private void GetGORefs()
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
        else
            EnableHitHurtBoxColliders(hitBoxCollider, true);

        hurtBoxCollider = transform.Find("HurtBox").GetComponent<Collider2D>();
        if (hurtBoxCollider == null)
            Debug.LogError("The HurtBox Collider Component was not found on any child of the gameobject " + gameObject);
        else
            EnableHitHurtBoxColliders(hurtBoxCollider, true);

        attackAreaCollider = transform.Find("AttackArea").GetComponent<Collider2D>();
        if (attackAreaCollider == null)
            Debug.LogError("The Attack Area Collider Component was not found on any child of the gameobject " + gameObject);
        else
        {
            // Save the initial World Pos. of the Attack Area Collider
            attackAreaColliderWorldPos = attackAreaCollider.transform.position;

            EnableAttackAreaCollider(true);            
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
            //Debug.Log("From " + currentState + " state to Death State. Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");

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
                    // Idle Timer Update
                    UdpateIdleTimer();

                    // State Update
                    if (isOnAttackArea)
                    {
                        ResetIdleTimer();
                        SetNextTargetPosition();

                        currentState = EnemyState.AlertPatrol;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (!isIdleTimerEnabled)
                    {
                        SetNextTargetPosition();

                        currentState = EnemyState.NormalPatrol;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.NormalPatrol:                                    
                    // Patrol Update
                    UpdateTargetPosition(0.01f);
                    Patrol(normalPatrolSpeed);

                    // State Update
                    if (isReachedPos)
                    {
                        SetIdleTimer();

                        currentState = EnemyState.Idle;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Normal Patrol states to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }                    
                    else if (isOnAttackArea)
                    {
                        SetCadenceAttackTimer();

                        currentState = EnemyState.AlertPatrol;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Normal Patrol state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.AlertPatrol:
                    // Idle Timer Update
                    UdpateCadenceAttackTimer();

                    // Patrol Update
                    UpdateTargetPosition(0.01f);
                    Patrol(alertPatrolSpeed);

                    // Set the next pos. every time the target pos is reached
                    if (isReachedPos)
                    {                        
                        SetNextTargetPosition();                                                
                    }

                    // State Update
                    if (!isOnAttackArea)
                    {
                        ResetCadenceAttackTimer();

                        currentState = EnemyState.NormalPatrol;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Alert Patrol state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (!isCadenceAttackTimerEnabled)
                    {           
                        SetAttackTimer();

                        currentState = EnemyState.Attack;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Alert Patrol state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Attack:
                    // Idle Timer Update
                    UdpateAttackTimer();

                    // State Update
                    if (!isAttackTimerEnabled)
                    {                        
                        if (!isOnAttackArea)                                                                                
                            currentState = EnemyState.NormalPatrol;                                                    
                        else 
                        {
                            SetCadenceAttackTimer();
                            currentState = EnemyState.AlertPatrol;                            
                        }

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Attack state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Death:
                    //Debug.Log("I am on Death State");
                    break;
                default:
                    // Default logic
                    break;
            }
        }
    }
    #endregion

    #region Timers
    #region Return To Idle Timer    
    private void UdpateIdleTimer()
    {
        // Idle Timer update
        idleTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (idleTimer <= 0)
        {
            ResetIdleTimer();
        }
    }
    private void ResetIdleTimer()
    {
        isIdleTimerEnabled = false;
        idleTimer = 0f;
    }
    private void SetIdleTimer()
    {
        idleTimer = idleMaxTime;
        isIdleTimerEnabled = true;
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
    #region Cadence Attack Timer       
    private void UdpateCadenceAttackTimer()
    {
        // Idle Timer update
        cadenceAttackTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (cadenceAttackTimer <= 0)
        {
            ResetCadenceAttackTimer();
        }
    }
    private void ResetCadenceAttackTimer()
    {
        isCadenceAttackTimerEnabled = false;
        cadenceAttackTimer = 0f;
    }
    private void SetCadenceAttackTimer()
    {
        cadenceAttackTimer = Random.Range(cadenceAttackMinTime, cadenceAttackMaxTime);
        //cadenceAttackTimer = cadenceAttackMaxTime;
        isCadenceAttackTimerEnabled = true;
    }
    #endregion
    #endregion

    #region Player Detection
    private void GetEnemyToPlayerDir()
    {
        // Update Enemy To Player Vector
        enemyToPlayerVector = (player.transform.position - transform.position);        
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
    private void EnableHitHurtBoxColliders(Collider2D collider, bool enable)
    {
        collider.enabled = enable;
    }
    private void EnableAttackAreaCollider(bool enable)
    {        
        attackAreaCollider.enabled = enable;
    }
    private void SetAttackAreaColliderPos()
    {
        // Set the correct offset for the Attack Collider 
        attackAreaCollider.transform.position = attackAreaColliderWorldPos;
    }
    private void UnsetAttackAreaColliderParent()
    {
        // Unset the parent of the Attack Area Collider 
        attackAreaCollider.transform.SetParent(null);
    }
    #endregion

    #region Projectiles
    private void InitProjectiles()
    {
        Vector2 originPosition;

        float initAngle = projectilesInitAngle * Mathf.Deg2Rad;
        float currentAngle = initAngle;
        //float incDeg = 90f * Mathf.Deg2Rad;
        projectilesAngleOffset = (360f/projectileNum);
        float incAngle = projectilesAngleOffset * Mathf.Deg2Rad;

        float initOffset = 0.5f;

        for (int i = 0; i < projectileNum; i++)
        {            
            currentAngle += (i == 0) ? 0f : incAngle;
            originPosition = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * initOffset;            

            projectiles[i] = InstantiateProjectilePrefabs(projectilePrefab, originPosition, transform);

            // Projectiles initial settings
            projectiles[i].InitPlayerRefs(playerHealth,player);
            projectiles[i].SetShootingSettings(cadenceAttackMinTime*0.9f,projectileAngleDelta);

            // Set an specific name to each projectile
            projectiles[i].name = "Projectile_" + i;

            // Disable the GO by def.
            projectiles[i].gameObject.SetActive(false);
        }
    }    
    private SpiderProjectile InstantiateProjectilePrefabs(GameObject prefab, Vector3 originPos, Transform parentTransform)
    {
        SpiderProjectile projectile = Instantiate(prefab, parentTransform).GetComponent<SpiderProjectile>();
        projectile.transform.localPosition = originPos;        
        projectile.transform.rotation = prefab.transform.rotation;                

        return projectile;

        //return Instantiate(prefab, originTransform.position, originTransform.rotation, parentTransform).
        //                GetComponent<ParticleSystem>();        
    }
    public void ShootProjectiles()
    {
        foreach (SpiderProjectile projectile in projectiles)
        {            
            projectile.gameObject.SetActive(true);
            projectile.SetIdleAnim();
            projectile.transform.SetParent(transform);
            projectile.Shooting();
        }
    }
    #endregion

    #region Attack
    #region Set Patrol
    private void SetInitPatrol()
    {
        // Set the Target Position Array and the Attack Speed
        if (patrolPoints == null)
            Debug.LogError("The Patrol Points GO does not contain any position references.");
        else
        {
            // Set the Patrol positions (origin and target) & the initial position                                    
            positions.originPosition = patrolPoints[0].position;
            positions.targetPosition = patrolPoints[1].position;

            transform.position = positions.originPosition;

            // Get the Attack and Apperance Animation Lengths in secs.
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            foreach (var clip in ac.animationClips)
            {
                if (clip.name == "Attack")
                    attackMaxTime = clip.length;
            }            
        }
    }
    #endregion
    #region Enemy Attack
    public void Attack()
    {
        // Get the Enemy's direction
        Vector2 enemyDirection = (player.transform.position.x <= transform.position.x) ?
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

        // Disable all the colliders
        EnableHitHurtBoxColliders(hitBoxCollider,false);
        EnableHitHurtBoxColliders(hurtBoxCollider,false);
        EnableAttackAreaCollider(false);

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
    void Patrol(float speed)
    {
        // Update the enemy's position
        Vector2 newPos = Vector3.MoveTowards(transform.position, positions.targetPosition, speed * Time.fixedDeltaTime);
        transform.position = newPos;
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
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Patrol");
        animator.ResetTrigger("Attack");      
        animator.ResetTrigger("Die");              
    }
    private void UpdateAnimations()
    {
        ClearAnimationFlags();

        switch (currentState)
        {
            case EnemyState.Idle:                
                StopAudioSource();
                PlayIdleSFX();

                animator.speed = DefaultAnimSpeed;
                animator.SetTrigger("Idle");                
                break;
            case EnemyState.NormalPatrol:
                StopAudioSource();
                PlayNormalPatrolSFX();

                animator.speed = DefaultAnimSpeed;
                animator.SetTrigger("Patrol");
                break;
            case EnemyState.AlertPatrol:
                StopAudioSource();
                PlayAlertPatrolSFX();

                animator.speed = alertPatrolAnimSpeed;
                animator.SetTrigger("Patrol");
                break;
            case EnemyState.Attack:
                StopAudioSource();
                PlayAttackSFX();

                animator.speed = DefaultAnimSpeed;
                animator.SetTrigger("Attack");
                break;
            case EnemyState.Death:
                animator.speed = DefaultAnimSpeed;
                animator.SetTrigger("Die");
                break;
            default:
                break;
        }
    }
    #endregion

    #region Audio
    public void PlayOneShotSFX(AudioClip clip)
    {
        //audioSource.PlayOneShot(clip);
    }
    public void PlayOnLoopSFX(AudioClip clip)
    {
        //audioSource.clip = clip;
        //audioSource.loop = true;
        //audioSource.Play();
    }
    public void StopAudioSource()
    {
        audioSource.Stop();
    }
    private void PlayIdleSFX()
    {
        PlayOnLoopSFX(idleSFX);
    }
    private void PlayAlertSFX()
    {
        PlayOneShotSFX(alertSFX);
    }
    private void PlayAttackSFX()
    {
        PlayOneShotSFX(attackSFX);
    }
    private void PlayDeathSFX()
    {
        PlayOneShotSFX(deathSFX);
    }        
    private void PlayNormalPatrolSFX()
    {
        PlayOnLoopSFX(normalPatrolSFX);
    }
    private void PlayAlertPatrolSFX()
    {
        PlayOnLoopSFX(alertPatrolSFX);
    }    
    #endregion
}
