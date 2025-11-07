using System.Collections;
using UnityEngine;
using static PlayerMovement;

public class Rat : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] pointsObjects;    // Points where I want my enemy to patrol
    Vector2[] points;                               // Patrol's points positions
    [SerializeField] private bool isReachedPos;
    [SerializeField] private AudioClip normalPatrolSFX;
    [SerializeField] private AudioClip alertPatrolSFX;
    [SerializeField] private AudioClip alertSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private float attackDistThreshold;    

    Vector3 targetPosition;
    int indexTargetPos;
    Rigidbody2D rb2D;

    [Header("Damage")]
    [SerializeField] private int damageAmount;    

    // Movement vars
    [Header("Movement")]
    [SerializeField] int walkingSpeed;           // Ant's normal speed    
    [SerializeField] int runningSpeed;           // Ant's boosted speed (whenever a player is detected)    
    int speed;                                  // Ant's current Speed

    [Header("Raycast")]    
    [SerializeField] LayerMask playerLayer;         // Player Layer
    [SerializeField] float pursuitDistance;         // Raycast Length
    [SerializeField] bool isDetecting;              // Player detection flag
    private bool wasDetecting;                      // Previous State of Player detection flag
    Vector2 raycastDir;
    private float enemyToPlayerDist;

    [Header("Player")]
    private GameObject player;
    private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement_ => playerMovement;
    private PlayerHealth playerHealth;
    private PlayerSFX playerSFX;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f
                                                        // Velocity --> 25f    
    [Header("Sprite")]
    [SerializeField] bool xPosDirSpriteValue;    // needed value on SpriteRenderer.flipX 
                                                 // to get a sprite looking to Vector2.right dir.

    [Header("Idle Timer")]
    [SerializeField] private float idleMaxTime;
    [SerializeField] private float idleTimer;
    [SerializeField] private bool isIdleTimerEnabled;

    [Header("Attack Timer")]
    [SerializeField] private float attackMaxTime;
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isAttackTimerEnabled;

    [Header("Death")]
    [SerializeField] private bool isDeath;
    [SerializeField] private AudioClip deathSFX;

    [Header("Respawn Timer")]
    [SerializeField] private float respawnMaxTime;
    [SerializeField] private float respawnTimer;
    [SerializeField] private bool isRespawnTimerEnabled;

    [Header("Respawn settings")]
    [SerializeField] private bool isOnRespawnDistance;
    [SerializeField] private float respawnDistance;

    public enum EnemyState
    {
        Idle,
        Walking,
        Running,        
        Attack,
        Death        
    }
    [Header("Enums")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    public EnemyState CurrentState => currentState;

    // Boolean Flags
    private bool isPlayerDetectionEnabled;    
    public bool IsPlayerDetectionEnabled => isPlayerDetectionEnabled;    

    // GOs 
    SpriteRenderer spriteRenderer;
    Animator animator;
    Animator scratchAnimator;
    SpriteRenderer scratchSprite;
    AudioSource audioSource;
    Collider2D bodyCollider;                                    // Non-Trigger collider used as the enemy's body and also collisions with the Player
    Collider2D receiveDamageCollider;                                 // Trigger collider used for detecting player attacks (ReceivePlayerAttack)
    Collider2D attackCollider;                                  // Trigger collider (on  GO's child) used as the attack collider (AttackPlayer)

    private Vector2 initAttackColliderOffset = Vector2.zero;
    private Vector2 initScratchVFXPos = Vector2.zero;

    #region Unity API
    void Awake()
    {
        // Set the initial speed
        speed = walkingSpeed;        

        // Get component
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb2D = GetComponent<Rigidbody2D>();

        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
            if (collider.isTrigger)
                receiveDamageCollider = collider;
            else
                bodyCollider = collider;
        }

        attackCollider = transform.Find("AttackCollider").GetComponent<Collider2D>();
        if (attackCollider == null)
            Debug.LogError("The AttackCollider Component was not found on any child of the gameobject " + gameObject);
        else
        {
            initAttackColliderOffset = attackCollider.offset;
        }

            // Get the Player's Reference
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
                // Set the initial flags        
                EnablePlayerDetection();

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

        scratchAnimator = transform.Find("ScratchVFX").GetComponent<Animator>();
        if (scratchAnimator == null)
            Debug.LogError("The Scratch Animator Component was not found on any child of the gameobject " + gameObject);
        else
        {
            initScratchVFXPos = scratchAnimator.transform.localPosition;
            scratchSprite = scratchAnimator.transform.GetComponent<SpriteRenderer>();
            if (scratchSprite == null)
                Debug.LogError("The Scratch VFX GO does not contain an Sprite Renderer component");
        }
            

        // Init the points Vector
        points = new Vector2[pointsObjects.Length];

        // Get all the patrol's points positions
        for (int i = 0; i < pointsObjects.Length; i++)        
            points[i] = new Vector2(pointsObjects[i].position.x,transform.position.y);
        // Set the initial Target Pos
        indexTargetPos = 0;
        // Set the initial patrol position
        targetPosition = points[indexTargetPos];        

        // Init raycastDir        
        raycastDir = Vector2.left;

        // Start Playing the default SFX
        PlayNormalPatrolSFX();
    }
    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    //DetectPlayer();        
    //}
    //private void Update_()
    //{
    //    // Check if the player has been detected
    //    DetectPlayer();

    //    // Update the Enemy's speed & anim's speed in func. of the player has been deteced or not
    //    if (isDetecting)
    //        AttackPlayer();
    //    else
    //        UpdateTargetPosition();
        
    //    //Patrol();
    //    FlipSprite();        
    //}
    //private void FixedUpdate_()
    //{
    //    Patrol();
    //}
    private void Update()
    {                
        FlipSprite();
    }
    private void FixedUpdate()
    {
        // Get the last State of isDetecting Flag
        wasDetecting = isDetecting;

        // Check if the player has been detected
        DetectPlayer();

        // Update the player state
        UpdateEnemyState();                        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") &&
            /*collision.collider.GetComponent<PlayerMovement>().IsGrounded &&*/
            isPlayerDetectionEnabled && !playerMovement.IsDead)
        {
            Attack();            

            DisablePlayerDetection();            
            Invoke(nameof(EnablePlayerDetection), 
                collision.collider.GetComponent<PlayerVFX>().FadingTotalDuration+2f);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isPlayerDetectionEnabled && !playerMovement.IsDead)
        {            
            ReceivePlayerAttack();
        }
    }
    #endregion
    #region Private Methods
    #region Enemy State
    // Enemy State
    private void UpdateEnemyState()
    {
        // From any state
        if (isDeath && currentState != EnemyState.Death)
        {
            // Play the SFX
            StopAudioSource();
            PlayDeathSFX();            

            // Set Rb as kinematics
            SetRBConfig(RigidbodyType2D.Kinematic);

            // Setup the GO destruction
            //Destroy(gameObject, 3f);

            // Debug
            //Debug.Log("From " + currentState + " state to Death State. Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");

            // Trigger Respawn Timer
            SetRespawnTimer();

            // State Update
            currentState = EnemyState.Death;

            // Anims update
            UpdateAnimations();            
        }
        else
        {
            switch (currentState)
            {
                case EnemyState.Death:
                    // Check constantly the Respawn Timer
                    if (isDeath)                    
                        CheckRespawnTimer();
                    else
                    {
                        currentState = EnemyState.Idle;
                        ResetAnimations();
                    }
                    //Debug.Log("From Death state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case EnemyState.Idle:
                    // Idle Timer Update
                    UdpateIdleTimer();

                    // State Update
                    if (isDetecting)
                    {
                        ResetIdleTimer();
                        SetNextTargetPosition(isDetecting);

                        currentState = EnemyState.Running;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (!isIdleTimerEnabled)
                    {
                        SetNextTargetPosition(isDetecting);

                        currentState = EnemyState.Walking;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Walking:
                    // Patrol Update
                    UpdateTargetPosition(0.01f);
                    Patrol(walkingSpeed);

                    // State Update
                    if (isReachedPos)
                    {
                        SetIdleTimer();

                        currentState = EnemyState.Idle;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Walking state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (isDetecting)
                    {
                        SetNextTargetPosition(isDetecting);

                        currentState = EnemyState.Running;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Walking state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Running:
                    // Patrol Update
                    UpdateTargetPosition(attackDistThreshold);
                    Patrol(runningSpeed);

                    // State Update
                    if (isReachedPos)
                    {
                        SetAttackTimer();

                        currentState = EnemyState.Attack;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Running state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    else if (!isDetecting)
                    {
                        SetNextTargetPosition(isDetecting);

                        currentState = EnemyState.Walking;

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Running state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Attack:
                    // Attack Collider will be triggered certain frames from the Attack Anim.

                    // Attack Timer Update
                    UdpateAttackTimer();

                    // Update Target Pos.
                    UpdateTargetPosition(attackDistThreshold);

                    if (!isAttackTimerEnabled)
                    {
                        // State Update
                        if (!isReachedPos)
                        {
                            SetNextTargetPosition(isDetecting);

                            if (isDetecting)
                            {
                                currentState = EnemyState.Running;

                                // Anims update
                                UpdateAnimations();

                                // Debug
                                //Debug.Log("From Attack state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                            }
                            else
                            {
                                currentState = EnemyState.Walking;

                                // Anims update
                                UpdateAnimations();

                                // Debug
                                //Debug.Log("From Attack state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                            }
                        }
                        else
                            SetAttackTimer();
                    }
                    break;                
                default:
                    // Default logic
                    break;
            }
        }
    }
    #endregion
    #region Respawn
    private void Respawn()
    {
        // Set the RB as Dynamics
        SetRBConfig(RigidbodyType2D.Dynamic);

        // Enable the both Enemy Colliders (Body & Receive Damage)
        EnableRatColliders(true);

        // Set the initial Target Pos
        indexTargetPos = 0;
        // Set the initial patrol position
        targetPosition = points[indexTargetPos];

        // Init raycastDir        
        raycastDir = Vector2.left;

        // Start Playing the default SFX
        PlayNormalPatrolSFX();        

        isDeath = false;
    }    
    private void CheckRespawnTimer()
    {
        if (isOnRespawnDistance)
        {
            if (!isRespawnTimerEnabled)
                SetRespawnTimer();
            else
                UpdateRespawnTimer();
        }
        else
        {
            if (isRespawnTimerEnabled)
                ResetRespawnTimer();
        }
    }
    #region Respawn Timer    
    private void UpdateRespawnTimer()
    {
        // Idle Timer update
        respawnTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (respawnTimer <= 0)
        {
            ResetRespawnTimer();
            Respawn();
        }
    }
    private void ResetRespawnTimer()
    {
        isRespawnTimerEnabled = false;
        respawnTimer = 0f;
    }
    private void SetRespawnTimer()
    {
        respawnTimer = respawnMaxTime;
        isRespawnTimerEnabled = true;
    }
    #endregion
    #endregion
    #region Idle Timer    
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
    public void EnableAttackCollider()
    {
        if (isDeath)
            return;

        StopAudioSource();
        PlayAttackSFX();

        // Set the RB as Kinematics
        SetRBConfig(RigidbodyType2D.Kinematic);

        // Disable the Body & Receive Damage Colliders
        EnableRatColliders(false);

        // Set the correct offset for the Attack Collider and enable it
        attackCollider.offset = spriteRenderer.flipX ? 
                                initAttackColliderOffset * Vector2.left :
                                initAttackColliderOffset;
        attackCollider.enabled = true;

        // Set the correct position of the Scratch VFX Anim and play it
        scratchAnimator.transform.localPosition = spriteRenderer.flipX ?
                                initScratchVFXPos * Vector2.left :
                                initScratchVFXPos;
        scratchSprite.flipX = spriteRenderer.flipX ? true : false;
        scratchSprite.enabled = true;

        scratchAnimator.SetTrigger("Scratch");
    }
    public void DisableAttackCollider()
    {
        // Disable again the scratch Collider VFX Sprite.
        scratchSprite.enabled = true;

        // Re-enable the Body & Receive Damage Colliders
        EnableRatColliders(true);

        // Set the RB as Dynamics
        SetRBConfig(RigidbodyType2D.Dynamic);

        // Disable the Attack Collider
        attackCollider.enabled = false;
    }
    private void EnableRatColliders(bool enable)
    {
        bodyCollider.enabled = enable;
        receiveDamageCollider.enabled = enable;
    }    
    private void SetRBConfig(RigidbodyType2D rbType)
    {
        rb2D.bodyType = rbType;
    }
    #endregion
    #region Player Detection
    // Raycast Detect Player Method
    //void DetectPlayer()
    //{
    //    // Update raycastDirection
    //    if (spriteRenderer.flipX)
    //        raycastDir = Vector2.right;
    //    else
    //        raycastDir = Vector2.left;

    //    // Raycast Launching
    //    isDetecting = Physics2D.Raycast(transform.position, raycastDir, pursuitDistance, playerLayer);
    //    // Raycast Debugging
    //    Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
    //}
    // No-Raycast Detec Player Method
    void DetectPlayer()
    {
        // Update raycastDirection
        if (spriteRenderer.flipX)
            raycastDir = Vector2.right;
        else
            raycastDir = Vector2.left;

        // Update the Enemy to Player Distance
        enemyToPlayerDist = Vector2.Distance(transform.position, player.transform.position);

        // Player on Pursuit Distance?
        isDetecting = (enemyToPlayerDist <= pursuitDistance) &&
                        (Mathf.Abs(transform.position.y - player.transform.position.y) <= 2f) /*&& 
                        playerDetectionEnabled*/
                        && !playerMovement.IsDead;

        // Player on Respawn Distance?
        isOnRespawnDistance = enemyToPlayerDist >= respawnDistance;

        // Raycast Debugging
        //Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
        Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * pursuitDistance, Color.red);
    }
    void EnablePlayerDetection()
    {
        //if(!playerMovement.IsDead)
        isPlayerDetectionEnabled = true;
    }
    public void DisablePlayerDetection()
    {
        isPlayerDetectionEnabled = false;
    }
    #endregion
    #region Player Attack
    private void ReceivePlayerAttack()
    {
        if (playerMovement.IsGrounded)
            return;

        playerMovement.UpwardsEnemyImpulse();
        playerSFX.PlayEnemyJumpSFX();

        // Disable the both Enemy Colliders (Body & Receive Damage)
        EnableRatColliders(false);

        isDeath = true;               
    }
    #endregion
    #region Enemy Movement
    void UpdateTargetPosition(float threshold)
    {        
        // Update the target pos with the player pos. (only for runing State)
        if ((currentState == EnemyState.Running || currentState == EnemyState.Attack) &&
            !playerMovement.IsDead)
            targetPosition.x = player.transform.position.x;

        // Check if the enemy reached its target position
        if ((Vector2.Distance(transform.position, targetPosition) < threshold) &&
            !playerMovement.IsDead)
            SetReachedPosFlag(true);
        else
            SetReachedPosFlag(false);
    }
    void SetNextTargetPosition(bool isPlayerDetected)
    {        
        if (isPlayerDetected)
            targetPosition.x = player.transform.position.x;
        else
        {
            if (indexTargetPos == points.Length - 1)
                indexTargetPos = 0;
            else
                indexTargetPos++;

            targetPosition.x = points[indexTargetPos].x;
        }

        SetReachedPosFlag(false);
    }    
    private void SetReachedPosFlag(bool enable)
    {
        isReachedPos = enable;
    }
    void Patrol(float speed)
    {
        // Update the ant's position
        Vector2 newPos = Vector3.MoveTowards(rb2D.position, targetPosition, speed * Time.fixedDeltaTime);
        rb2D.MovePosition(newPos);
    }
    #endregion
    #region Sprite & Animations
    // Flip the Enemy's sprite in function of its movement
    void FlipSprite()
    {        
        if (targetPosition.x > transform.position.x)        
            spriteRenderer.flipX = xPosDirSpriteValue;
        else
            spriteRenderer.flipX = !xPosDirSpriteValue;
    }
    private void ClearAnimationFlags()
    {
        animator.ResetTrigger("IsIdle");
        animator.ResetTrigger("IsRunning");
        animator.ResetTrigger("IsWalking");        
        animator.ResetTrigger("IsAttacking");
        animator.ResetTrigger("IsDying");
    }
    private void UpdateAnimations()
    {
        ClearAnimationFlags();

        switch (currentState)
        {
            case EnemyState.Idle:
                animator.SetTrigger("IsIdle");
                break;
            case EnemyState.Walking:
                StopAudioSource();
                PlayNormalPatrolSFX();
                animator.SetTrigger("IsWalking");                
                break;
            case EnemyState.Running:
                StopAudioSource();
                PlayAlertSFX();
                //StartCoroutine(nameof(PlayAlertWhenReady));
                PlayAlertPatrolSFX();
                animator.SetTrigger("IsRunning");
                break;                        
            case EnemyState.Attack:
                animator.SetTrigger("IsAttacking");
                break;
            case EnemyState.Death:
                animator.SetTrigger("IsDying");
                break;
            default:
                break;
        }        
    }
    private void ResetAnimations()
    {
        animator.Rebind();
        animator.Update(0f);
    }
    #endregion
    #endregion
    #region Public Methods    
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
    private void PlayDeathSFX()
    {
        PlayOneShotSFX(deathSFX);
    }
    public void PlayAttackSFX()
    {
        PlayOneShotSFX(attackSFX);
    }
    public void PlayAlertSFX()
    {
        PlayOneShotSFX(alertSFX);
    }    
    private void PlayNormalPatrolSFX()
    {
        PlayOnLoopSFX(normalPatrolSFX);
    }
    private void PlayAlertPatrolSFX()
    {
        PlayOnLoopSFX(alertPatrolSFX);
    }
    private IEnumerator PlayAlertWhenReady()
    {
        yield return new WaitUntil(()=>!audioSource.isPlaying);        

        if (currentState == EnemyState.Running)
            PlayAlertPatrolSFX();
    }
    #endregion
    #endregion
}
