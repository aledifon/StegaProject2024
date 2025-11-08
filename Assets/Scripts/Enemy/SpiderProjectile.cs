using Demo_Project;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SpiderProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damageAmount;
    [SerializeField] private float thrustToPlayer;

    [Header("Shooting Speed")]
    [SerializeField] private float speed;

    [Header("SFX")]
    [SerializeField] private AudioClip shootSFX;    
    [SerializeField] private AudioClip explodeSFX;
    
    private float shootingMaxTime;
    private float shootingTimer;
    private bool isShootingTimerEnabled;
    
    private Vector2 shootingDir;
    private Vector2 shootingDirRotated;
    private float shootAngleInc;

    private Collider2D collider;
    private GameObject player;
    private PlayerHealth playerHealth;
    private Animator anim;
    private SpriteRenderer sprite;
    private AudioSource audioSource;

    #region Unity API
    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        if (isShootingTimerEnabled)
        {           
            // Movement update only enabled if no collision has happened.
            if(collider.enabled)
                transform.position += (Vector3)shootingDir.normalized * speed * Time.deltaTime;

            UdpateShootingTimer();
        }

        string str = gameObject.activeSelf ? "enabled" : "disabled";        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collider.enabled = false;            
            SetExplodeAnim();
            Attack();
            audioSource.PlayOneShot(explodeSFX);
        }
    }
    #endregion

    #region Attack
    private void Attack()
    {
        // Get the Enemy's direction
        Vector2 projectileDir = (player.transform.position.x <= transform.position.x) ?
                                Vector2.left + Vector2.up :
                                Vector2.right + Vector2.up;

        // Take Player's Damage & Disable the player's detection for a certain time            
        playerHealth.TakeDamage(damageAmount, projectileDir, thrustToPlayer);
    }
    #endregion

    #region Player refs
    private void SetPlayerHealthRef(PlayerHealth pH)
    {
        playerHealth = pH;
    }
    private void SetPlayerRef(GameObject GO_player)
    {
        player = GO_player;
    }
    #endregion

    #region Disable GO
    private void DisableGO()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region Reset State
    private void ReturnToInitState()
    {
        //Calculate Rotated Vector
        int randomChoice = Random.Range(0,3);

        if (randomChoice == 0)
            shootingDirRotated = shootingDir;
        else if (randomChoice == 1)
            shootingDirRotated = Quaternion.Euler(0, 0, shootAngleInc) * shootingDir;
        else if (randomChoice == 2)
            shootingDirRotated = Quaternion.Euler(0, 0, -shootAngleInc) * shootingDir;

        // Alternative shootingDir Update Method        
        //float randomAngleInc = Random.Range(-shootAngleInc, shootAngleInc);        
        //shootingDirRotated = Quaternion.Euler(0, 0, shootAngleInc) * shootingDir;

        transform.localPosition = shootingDirRotated;
        collider.enabled = true;

        //if (gameObject.name == "Projectile_3")
        //    Debug.Log("Pos = " + transform.localPosition);
    }
    #endregion    

    #region Vanishing Timer    
    private void UdpateShootingTimer()
    {
        // Idle Timer update
        shootingTimer -= Time.fixedDeltaTime;

        // Reset Idle Timer
        if (shootingTimer <= 0)
        {
            ResetShootingTimer();
        }
    }
    private void ResetShootingTimer()
    {
        isShootingTimerEnabled = false;
        shootingTimer = 0f;
        
        DisableGO();
        //else
        //    Invoke(nameof(DisableGO), 3f);
    }    
    private void SetShootingTimer()
    {
        shootingTimer = shootingMaxTime;
        isShootingTimerEnabled = true;
    }
    #endregion

    #region Shooting
    public void SetShootingSettings(float maxShootTime, float shootAngle)
    {
        shootingDir = transform.localPosition;
        shootingMaxTime = maxShootTime;
        shootAngleInc = shootAngle;
    }
    public void Shooting()
    {
        ReturnToInitState();
        transform.SetParent(null);
        SetShootingTimer();
        audioSource.PlayOneShot(shootSFX);
    }
    #endregion

    #region Init Refs
    public void InitPlayerRefs(PlayerHealth pH, GameObject GO_player)
    {
        SetPlayerHealthRef(pH);
        SetPlayerRef(GO_player);
    }
    #endregion

    #region Animations
    public void SetIdleAnim()
    {        
        //sprite.enabled = true;
        anim.ResetTrigger("Explode");
        anim.SetTrigger("Idle");        
    }
    public void SetExplodeAnim()
    {
        anim.ResetTrigger("Idle");
        anim.SetTrigger("Explode");
    }
    #endregion    
}
