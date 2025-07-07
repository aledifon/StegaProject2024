using UnityEngine;
using System;

public class EnemyMovement : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] pointsObjects;    // Points where I want my enemy to patrol
    Vector2[] points;                               // Patrol's points positions

    Vector3 targetPosition;
    int indexTargetPos;

    [Header("Damage")]
    [SerializeField] private int damageAmount;    

    // Movement vars
    [Header("Movement")]
    [SerializeField] int walkingSpeed;           // Ant's normal speed    
    [SerializeField] int attackSpeed;           // Ant's boosted speed (whenever a player is detected)
    [SerializeField] int animAttackSpeed;
    int speed;                                  // Ant's current Speed

    [Header("Raycast")]    
    [SerializeField] LayerMask playerLayer;         // Player Layer
    [SerializeField] float pursuitDistance;         // Raycast Length
    [SerializeField] bool isDetecting;              // Player detection flag
    Vector2 raycastDir;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f

    // Boolean Flags
    private bool playerDetectionEnabled;    

    // GOs 
    SpriteRenderer spriteRenderer;
    Animator anim;
    AudioSource audioSource;

    #region Unity API
    void Awake()
    {
        // Set the initial speed
        speed = walkingSpeed;

        // Set the initial flags        
        EnablePlayerDetection();

        // Get component
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Init the points Vector
        points = new Vector2[pointsObjects.Length];

        // Get all the patrol's points positions
        for (int i = 0; i < pointsObjects.Length; i++)        
            points[i] = pointsObjects[i].position;
        // Set the initial Target Pos
        indexTargetPos = 0;
        // Set the initial patrol position
        targetPosition = points[indexTargetPos];

        // Init raycastDir        
        raycastDir = Vector2.left;
    }
    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    //DetectPlayer();        
    //}
    private void Update()
    {
        // Check if the player has been detected
        DetectPlayer();

        // Update the Enemy's speed & anim's speed in func. of the player has been deteced or not
        if (isDetecting)
            AttackPlayer();
        else
            UpdateTargetPosition();
        
        Patrol();
        FlipSprite();        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && 
            collision.collider.GetComponent<PlayerMovement>().IsGrounded &&
            playerDetectionEnabled)
        {
            // Get the Enemy's direction
            Vector2 enemyDirection = spriteRenderer.flipX ? 
                                    Vector2.right + Vector2.up: 
                                    Vector2.left + Vector2.up;            

            // Take Player's Damage & Disable the player's detection for a certain time            
            collision.collider.GetComponent<PlayerHealth>().TakeDamage(damageAmount, enemyDirection, thrustToPlayer);

            DisablePlayerDetection();
            SetNextTargetPosition();
            Invoke(nameof(EnablePlayerDetection), 
                collision.collider.GetComponent<PlayerVFX>().FadingTotalDuration+2f);
        }
    }
    #endregion
    #region Private Methods
    #region Enemy Attack
    private void AttackPlayer()
    {
        speed = attackSpeed;
        anim.speed = animAttackSpeed;
        targetPosition = new Vector2(player.transform.position.x, targetPosition.y);
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

        // Raycast Launching
        isDetecting = (Vector2.Distance(transform.position, player.transform.position) <= pursuitDistance) &&
                        playerDetectionEnabled;
        // Raycast Debugging
        Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
    }
    void EnablePlayerDetection()
    {
        playerDetectionEnabled = true;
    }
    void DisablePlayerDetection()
    {
        playerDetectionEnabled = false;
    }
    #endregion
    #region Enemy Movement
    void UpdateTargetPosition()
    {
        speed = walkingSpeed;
        anim.speed = 1;         // Equivalent to the num of samples already set on current animation

        // Update the patrol target points
        if (Vector2.Distance(transform.position, targetPosition) < Mathf.Epsilon)
        {
            SetNextTargetPosition();
        }
    }
    void SetNextTargetPosition()
    {
        if (indexTargetPos == points.Length - 1)
            indexTargetPos = 0;
        else
            indexTargetPos++;

        targetPosition = points[indexTargetPos];
    }
    void Patrol()
    {        
        // Update the ant's position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);        
    }
    #endregion
    #region Sprite
    // Flip the Enemy's sprite in function of its movement
    void FlipSprite()
    {        
        if (targetPosition.x > transform.position.x)        
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }
    #endregion
    #endregion
    #region Public Methods
    #region Audio
    public void PlayDeathFx()
    {
        audioSource.Play();
    }
    #endregion
    #endregion
}
