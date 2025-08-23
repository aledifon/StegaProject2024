using UnityEngine;
using System;

public class EnemyMovement_ : MonoBehaviour
{
    // Class vars.
    [Header("Patrol points")]
    [SerializeField] Transform [] pointsObjects;    // Points where I want my enemy to patrol
    Vector2[] points;                               // Patrol's points positions

    Vector3 targetPosition;
    int indexTargetPos;
    Rigidbody2D rb2D;

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
    private bool wasDetecting;                      // Previous State of Player detection flag
    Vector2 raycastDir;

    [Header("Player")]
    private GameObject player;
    [SerializeField] private float thrustToPlayer;      // ForceMode2D = Impulse --> 3-4f
                                                        // ForceMode2D = Force --> 250-300f
                                                        // Velocity --> 25f    

    [Header("Sprite")]
    [SerializeField] bool xPosDirSpriteValue;    // needed value on SpriteRenderer.flipX 
                                                        // to get a sprite looking to Vector2.right dir.

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
        rb2D = GetComponent<Rigidbody2D>();

        // Get the Player's Reference
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("The Enemy can't find the GO Player");

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
        // Check if the player has been detected
        DetectPlayer();        

        //Patrol();
        FlipSprite();
    }
    private void FixedUpdate()
    {
        // Update the Enemy's speed & anim's speed in func. of the player has been deteced or not
        if (isDetecting)
            AttackPlayer();
        else if (wasDetecting && !isDetecting)
            SetNextTargetPosition();
        else
            UpdateTargetPosition();

        Patrol();

        // Get the last State of isDetecting Flag
        wasDetecting = isDetecting;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") &&
            /*collision.collider.GetComponent<PlayerMovement>().IsGrounded &&*/
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
                        (Mathf.Abs(transform.position.y - player.transform.position.y) <= 2f) && 
                        playerDetectionEnabled;
        // Raycast Debugging
        //Debug.DrawRay(transform.position, raycastDir * pursuitDistance, Color.red);
        Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * pursuitDistance, Color.red);
    }
    void EnablePlayerDetection()
    {
        playerDetectionEnabled = true;
    }
    public void DisablePlayerDetection()
    {
        playerDetectionEnabled = false;
    }
    #endregion    
    #region Enemy Movement
    void UpdateTargetPosition()
    {
        speed = walkingSpeed;
        anim.speed = 1;         // Equivalent to the num of samples already set on current animation
        float threshold = 0.01f;

        // Update the patrol target points
        if (Vector2.Distance(transform.position, targetPosition) < threshold)
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
    //void Patrol_()
    //{        
    //    // Update the ant's position
    //    transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);        
    //}
    void Patrol()
    {
        // Update the ant's position
        Vector2 newPos = Vector3.MoveTowards(rb2D.position, targetPosition, speed * Time.fixedDeltaTime);
        rb2D.MovePosition(newPos);
    }
    #endregion
    #region Sprite
    // Flip the Enemy's sprite in function of its movement
    void FlipSprite()
    {        
        if (targetPosition.x > transform.position.x)        
            spriteRenderer.flipX = xPosDirSpriteValue;
        else
            spriteRenderer.flipX = !xPosDirSpriteValue;
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
