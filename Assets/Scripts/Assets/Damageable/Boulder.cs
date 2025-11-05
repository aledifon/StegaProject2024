using Demo_Project;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    Animator animator;
    AudioSource audioSource;
    //CircleCollider2D ballCollider;
    BoxCollider2D detectPlayerCollider;
    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;

    [Header("Dynamic Settings")]
    [SerializeField] float gravityScale;
    [SerializeField] float mass;

    [Header("Audio")]
    [SerializeField] AudioClip clip;

    //[Header("Movement")]
    //[SerializeField] float speed;

    [Header("VFX")]
    [SerializeField] private GameObject dustVFX;
    [SerializeField] Transform origindustPS;
    private ParticleSystem dustPS;

    bool isPlayerCaught = false;
    bool isPlayerDetected = false;
    bool isRolling = false;
    bool isStopped = false;

    // Spawn Positions
    Transform spawnInitPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        //ballCollider = GetComponent<CircleCollider2D>();
        detectPlayerCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("The GO has not a SpriteRenderer component");
        else
        {
            spriteRenderer.enabled = false;
        }

        // VFX
        if (dustVFX == null)
            Debug.LogError("No Sparks VFX were added on the Inspector");
        else
        {
            dustPS = InstantiateVFXPrefabs(dustVFX, origindustPS, transform);
        }

        spawnInitPos = GameObject.Find("BoulderSpawnInitPos")?.transform;
        if (spawnInitPos == null)
            Debug.LogError("None BoulderSpawnInitPos was found on the Scene");
        else
            transform.position = spawnInitPos.position;

        // Setup AudioSource and play
        audioSource.loop = true;
        audioSource.clip = clip;                
    }
    private void Update()
    {
        //if (isRolling)
        //    Debug.Log("Angular velocity = "+rb2D.angularVelocity +
        //                "|| Lin. Velocity = " + rb2D.linearVelocity.magnitude);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Slope") && !isRolling)
        {
            isRolling = true;

            // Start playing the VFX & the SFX
            PlayVFX(dustPS);
            audioSource.Play();

            // Start the Rolling Animation
            animator.SetTrigger("Turn");
        }            
        else if (collision.collider.CompareTag("Player") && !isPlayerCaught)
        {
            isPlayerCaught = true;

            Debug.Log("Player Detected!");

            // Player Death
            collision.collider.GetComponent<PlayerHealth>().TakeDamage(100, Vector2.up, 50f);
        }        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EndRolling") && !isStopped)
        {
            isStopped = true;

            // Stop VFX, SFX & came back to Idle anim.
            StopVFX(dustPS);
            audioSource.Stop();
            animator.SetTrigger("StopTurn");

            //Stop the Boulder
            StartCoroutine(nameof(StopRock));
            //SetRBAsKinematic();
        }
        else if (collision.gameObject.CompareTag("Player") && !isPlayerDetected)
        {
            isPlayerDetected = true;

            // Show the Boulder sprite & Make it fall
            spriteRenderer.enabled = true;
            SetRBAsDynamic();

            // Disable the Player Detection Collider (to avoid stopping the rock too early)
            detectPlayerCollider.enabled = false;
        }
    }
    private ParticleSystem InstantiateVFXPrefabs(GameObject prefab, Transform originTransform, Transform parentTransform)
    {
        ParticleSystem ps = Instantiate(prefab, parentTransform).GetComponent<ParticleSystem>();

        ps.transform.localPosition = originTransform.localPosition;
        ps.transform.localRotation = prefab.transform.rotation;
        //ps.transform.localRotation *= Quaternion.Euler(30f,0f,0f);

        return ps;

        //return Instantiate(prefab, originTransform.position, originTransform.rotation, parentTransform).
        //                GetComponent<ParticleSystem>();        
    }
    private void PlayVFX(ParticleSystem ps)
    {
        //if (!ps.isPlaying)
        //    ps.Play();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }
    private void StopVFX(ParticleSystem ps)
    {
        if (ps.isPlaying)
            ps.Stop();
    }

    private IEnumerator StopRock()
    {
        yield return new WaitUntil(() =>
            rb2D.angularVelocity <= 0.01f &&
            rb2D.linearVelocity.magnitude <= 0.01f
        );
        //yield return new WaitForSeconds(2f);
        SetRBAsKinematic();
    }
    private void SetRBAsKinematic()
    {
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
        rb2D.bodyType = RigidbodyType2D.Kinematic;
    }
    private void SetRBAsDynamic()
    {
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.mass = mass;
        rb2D.gravityScale = gravityScale;
    }
}
