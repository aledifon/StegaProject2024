using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static PlayerMovement;

public class PlantCarnivore : MonoBehaviour
{    
    [Header("Damage")]
    [SerializeField] private int damageAmount;    

    [Header("Raycast")]
    [SerializeField] LayerMask playerLayer;         // Player Layer    
    [SerializeField] float attackHorizRange;      // Raycast Length for attack's area    
    [SerializeField] bool isOnAttackRange;           // Player is on Attack Area flag
    Vector2 enemyToPlayerVector;

    [Header("Player")]
    private GameObject player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerSFX playerSFX;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f
                                                        // Velocity --> 25f        
    
    [Header("Attack Timer")]
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isAttackTimerEnabled;
    private float attackMaxTime;                                // Calculated in func. of the Attack Anim Length        
    [SerializeField] private float waitForAttackMaxTime;
    [SerializeField] private float waitForAttackTimer;
    [SerializeField] private bool isWaitForAttackTimerEnabled;        

    [Header("Death")]
    [SerializeField] private bool isDeath;    

    [Header("SFX")]
    [SerializeField] private AudioClip idleSFX;        
    [SerializeField] private AudioClip attackSFX;
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
        Attack,        
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

    #region Unity API    
    void Awake()
    {
        GetGORefs();        

        // Player detection will be always enabled except for a few secs. when the player is detected.
        EnablePlayerDetection();

        // Set the Wait For Attack Timer
        SetWaitForAttackTimer();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // Get Player To Enemy Dir. Vector and the Sprite-X Direction
        GetEnemyToPlayerDir();

        // Detect Player on DetectArea (Idle -> Appearance -> IdleAlert)        
        CheckDetectionArea();

        // Detect Player on AttackArea (IdleAlert -> Attack)
        // Managed through 'AttackArea' BoxCollider2D.        

        // Update the Enemy state
        UpdateEnemyState();

        // Animations
        UpdateAnimationsNew();
    }
    public void OnChildTriggerEnter(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player") && isPlayerDetectionEnabled && !playerMovement.IsDead)
        {
            Debug.Log("Player entered on the HitBox");

            DisablePlayerDetection();
            Invoke(nameof(EnablePlayerDetection),1f);

            Attack();
        }
        else if (id == "HurtBox" && collision.CompareTag("Player") && isPlayerDetectionEnabled && !playerMovement.IsDead)
        {
            if (playerMovement.IsGrounded)
                return;

            Debug.Log("Player entered on the HurtBox");

            DisablePlayerDetection();

            ReceivePlayerAttack();
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
        else
        {
            // Get the Attack and Apperance Animation Lengths in secs.
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            foreach (var clip in ac.animationClips)
            {
                if (clip.name == "Attack" || clip.name == "AttackLeft")
                    attackMaxTime = clip.length;                
            }
        }

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
            EnableHitHurtBoxColliders(hurtBoxCollider, false);       

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
            //Destroy(gameObject, 3f);

            // Debug
            //Debug.Log("From " + currentState + " state to Death State. Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");

            // Trigger Respawn Timer
            SetRespawnTimer();

            // State Update
            currentState = EnemyState.Death;

            UpdateTriggerAnimations();

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

                        //UpdateTriggerAnimations();
                    }
                    //Debug.Log("From Death state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    break;
                case EnemyState.Idle:
                    // Wait for Attack Timer Update (As long as is enabled)
                    if(isWaitForAttackTimerEnabled)
                        UdpateWaitForAttackTimer();

                    // State Update
                    if (isOnAttackRange && !isWaitForAttackTimerEnabled)
                    {
                        // Set the Appearance Timer
                        SetAttackTimer();

                        // Update the state
                        currentState = EnemyState.Attack;

                        StopAudioSource();
                        Invoke(nameof(PlayAttackSFX), 0.1f);

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Idle state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
                    }
                    break;
                case EnemyState.Attack:
                    // Idle Timer Update
                    UdpateAttackTimer();

                    // State Update
                    if (!isAttackTimerEnabled)
                    {                        
                        SetWaitForAttackTimer();
                        currentState = EnemyState.Idle;

                        // Play the Idle SFX
                        StopAudioSource();
                        PlayIdleSFX();

                        // Anims update
                        UpdateAnimations();

                        // Debug
                        //Debug.Log("From Attack state to " + currentState + ". Time: " + (Time.realtimeSinceStartup * 1000f) + "ms");
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
        // Set the Colliders on its init state
        EnableHitHurtBoxColliders(hitBoxCollider, true);
        EnableHitHurtBoxColliders(hurtBoxCollider, false);

        // Reset the transform Scale to its init value (Was set to 0 at the end of 'Death' anim)
        transform.localScale = Vector3.one;

        // Player detection will be always enabled except for a few secs. when the player is detected.
        EnablePlayerDetection();

        // Set the Wait For Attack Timer
        SetWaitForAttackTimer();        

        // Play the Idle SFX
        PlayIdleSFX();

        // Respawn the character (Trigger from Death->Idle State)
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
    #region Player Detection
    private void GetEnemyToPlayerDir()
    {
        // Update Enemy To Player Vector
        enemyToPlayerVector = (player.transform.position - transform.position);
    }
    private void CheckDetectionArea()
    {
        // Raycast Launching
        isOnAttackRange = Physics2D.Raycast(transform.position, enemyToPlayerVector.normalized, attackHorizRange, playerLayer) &&
                          (spriteRenderer.flipX ? 
                          (player.transform.position.x < transform.position.x) : 
                          (player.transform.position.x > transform.position.x)) 
                          && !playerMovement.IsDead;

        // Player on Respawn Distance?
        isOnRespawnDistance = enemyToPlayerVector.magnitude >= respawnDistance;

        // Raycast Debugging
        //Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
        Debug.DrawRay(transform.position, enemyToPlayerVector.normalized * attackHorizRange, Color.red);
    }
    void EnablePlayerDetection()
    {
        //if (!playerMovement.IsDead)
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
    public void EnableHurtBoxCollider()
    {
        if (isDeath)
            return;

        EnableHitHurtBoxColliders(hurtBoxCollider,true);
    }
    public void DisableHurtBoxCollider()
    {
        if (isDeath)
            return;

        EnableHitHurtBoxColliders(hurtBoxCollider,false);
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
        //if (playerMovement.IsGrounded)
        //    return;

        playerMovement.UpwardsEnemyImpulse();
        playerSFX.PlayEnemyJumpSFX();

        // Disable all the colliders
        EnableHitHurtBoxColliders(hitBoxCollider, false);
        EnableHitHurtBoxColliders(hurtBoxCollider, false);        

        isDeath = true;
    }
    #endregion    
    #endregion

    #region Timers
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
    #endregion

    #region Sprite & Animations    
    //private void ClearAnimationFlags()
    //{
    //    animator.ResetTrigger("Die");                
    //}
    private void UpdateTriggerAnimations()
    {
        //ClearAnimationFlags();

        switch (currentState)
        {            
            case EnemyState.Death:
                animator.SetTrigger("Die");
                break;
            default:
                break;
        }
    }
    private void UpdateAnimationsNew()
    {
        // IDLE        
        bool isIdle = (!isOnAttackRange && isWaitForAttackTimerEnabled) &&
                    !isDeath;
        animator.SetBool("IsIdle", isIdle);

        // ATTACK
        bool isAttack = (isOnAttackRange && isAttackTimerEnabled) &&
                        !isDeath;
        animator.SetBool("IsAttack", isAttack);
    }
    private void UpdateAnimations()
    {
        return;

        //ClearAnimationFlags();

        switch (currentState)
        {
            case EnemyState.Idle:
                //animator.SetTrigger("Idle");
                // Reset the other triggers
                animator.ResetTrigger("Die");
                animator.ResetTrigger("Attack");

                //StopAudioSource();
                //PlayIdleSFX();
                break;
            case EnemyState.Attack:
                // Reset the another trigger
                animator.ResetTrigger("Die");                

                animator.SetTrigger("Attack");
                //StopAudioSource();
                //Invoke(nameof(PlayAttackSFX), 0.1f);
                //PlayAttackSFX();                
                break;
            case EnemyState.Death:
                // Reset the another trigger
                animator.ResetTrigger("Attack");

                animator.SetTrigger("Die");
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
