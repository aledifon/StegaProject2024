using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Windows;
using static PlayerMovement;

public class Bat : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] pointsObjects;    // Points where I want my enemy to patrol
    Vector2[] points;                               // Patrol's points positions
    [SerializeField] private bool isReachedPos;
    Vector3 targetPosition;
    int indexTargetPos;

    [SerializeField] private float attackDistThreshold;          

    [Header("Damage")]
    [SerializeField] private int damageAmount;    

    // Movement vars
    [Header("Movement")]
    [SerializeField] int walkingSpeed;           // Ant's normal speed            

    [Header("Raycast")]    
    [SerializeField] LayerMask playerLayer;         // Player Layer
    [SerializeField] float rayLength;           //Raycast Length
    [SerializeField] float detectAreaDistance;      // Raycast Length for player's detection
    [SerializeField] float attackAreaDistance;      // Raycast Length for attack's area
    [SerializeField] bool isOnDetectArea;           // Player is on detection Area flag
    [SerializeField] bool isOnAttackArea;           // Player is on detection Area flag
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

    [Header("Idle Timer")]
    [SerializeField] private float IdleMaxTime;
    [SerializeField] private float idleTimer;
    [SerializeField] private bool isIdleTimerEnabled;

    [Header("Attack Timer")]
    [SerializeField] private float AttackMaxTime;
    [SerializeField] private float attackTimer;
    [SerializeField] private bool isAttackTimerEnabled;

    [Header("Death")]
    [SerializeField] private bool isDeath;

    [Header("SFX")]
    [SerializeField] private AudioClip idleSFX;
    [SerializeField] private AudioClip alertSFX;
    [SerializeField] private AudioClip idleAlertSFX;
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip deathSFX;

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
    private bool playerDetectionEnabled;    

    // GOs 
    SpriteRenderer spriteRenderer;
    Animator animator;
    //Animator scratchAnimator;
    //SpriteRenderer scratchSprite;
    AudioSource audioSource;    
    Collider2D receiveDamageCollider;                           // Trigger collider used for detecting player attacks (ReceivePlayerAttack)
    Collider2D attackCollider;                                  // Trigger collider (on  GO's child) used as the attack collider (AttackPlayer)
    Rigidbody2D rb2D;

    private Vector2 initAttackColliderOffset = Vector2.zero;
    private Vector2 initScratchVFXPos = Vector2.zero;

    #region Unity API
    void Awake()
    {
        
    }    
    private void Update()
    {                        

    }
    private void FixedUpdate()
    {
        GetEnemyToPlayerDir();

        // Detect Player on DetectArea (Idle -> Appearance -> IdleAlert)        
        CheckDetectionArea();

        // Detect Player on AttackArea (IdleAlert -> Attack)
        // Raycast solo en x
        CheckAttackArea();

        // Patrol (Circle Arc from current->target A->B or B->A)

        // Update Enemy State                    
    }

    public void OnChildTriggerEnter(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player entró en el HitBox");
        }
        else if (id == "HurtBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player entró en el HurtBox");
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = true;
            Debug.Log("Player entró en la AttackArea");
        }
    }
    public void OnChildTriggerExit(string id, Collider2D collision)
    {
        if (id == "HitBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player salió del HitBox");
        }
        else if (id == "HurtBox" && collision.CompareTag("Player"))
        {
            Debug.Log("Player salió del HurtBox");
        }
        else if (id == "AttackArea" && collision.CompareTag("Player"))
        {
            isOnAttackArea = false;
            Debug.Log("Player salio de la AttackArea");
        }
    }
    #endregion

    #region Player Detection
    private void GetEnemyToPlayerDir()
    {
        // Update Enemy To Player Vector
        enemyToPlayerVector = (player.transform.position - transform.position);

        // Update Sprite X-Flip        
        spriteRenderer.flipX = (enemyToPlayerVector.x >= 0) ? false : true;
    }
    private void CheckDetectionArea()
    {        
        // Detection Area flag update
        isOnDetectArea = (Vector2.Distance(transform.position, player.transform.position) <= detectAreaDistance);

        // Raycast Debugging
        //Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
        Debug.DrawRay(transform.position, enemyToPlayerVector.normalized * detectAreaDistance, Color.red);
    }
    //private void CheckAttackArea()
    //{           
    //    RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position,enemyToPlayerVector.normalized, attackAreaDistance, playerLayer);

    //    isOnAttackArea = raycastHit2D && Mathf.Abs(enemyToPlayerVector.x) <= attackAreaDistance;

    //    Debug.DrawRay(transform.position, enemyToPlayerVector.normalized * rayLength, Color.blue);
    //}
    #endregion

    #region Sprite & Animations
    void FlipSprite()
    {
        // If targetPos B -> spriteRenderer.flipX = true;
        // Else if targetPos = A --> spriteRenderer.flipX = false;        
    }
    //private void ClearAnimationFlags()
    //{
    //    animator.ResetTrigger("IsIdle");
    //    animator.ResetTrigger("IsRunning");
    //    animator.ResetTrigger("IsWalking");
    //    animator.ResetTrigger("IsAttacking");
    //    animator.ResetTrigger("IsDying");
    //}
    //private void UpdateAnimations()
    //{
    //    ClearAnimationFlags();

    //    switch (currentState)
    //    {
    //        case EnemyState.Idle:
    //            animator.SetTrigger("IsIdle");
    //            break;
    //        case EnemyState.Walking:
    //            StopAudioSource();
    //            PlayNormalPatrolSFX();
    //            animator.SetTrigger("IsWalking");
    //            break;
    //        case EnemyState.Running:
    //            StopAudioSource();
    //            PlayAlertSFX();
    //            //StartCoroutine(nameof(PlayAlertWhenReady));
    //            PlayAlertPatrolSFX();
    //            animator.SetTrigger("IsRunning");
    //            break;
    //        case EnemyState.Attack:
    //            animator.SetTrigger("IsAttacking");
    //            break;
    //        case EnemyState.Death:
    //            animator.SetTrigger("IsDying");
    //            break;
    //        default:
    //            break;
    //    }
    //}
    #endregion
}
