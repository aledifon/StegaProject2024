using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class GemCollectible : MonoBehaviour
{
    //Animator animator;
    AudioSource audioSource;
    SpriteRenderer spriteRenderer;
    bool isCaptured = false;

    [SerializeField] AudioClip clip;       

    private Collider2D myCollider;        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Get components on the parent GO        
        audioSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider2D>();      
        spriteRenderer = GetComponent<SpriteRenderer>();
    }        
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCaptured)
        {
            myCollider.enabled = false;
            spriteRenderer.enabled = false;
            isCaptured = true;

            audioSource.PlayOneShot(clip);

            collision.gameObject.GetComponent<PlayerMovement>().IncreaseAcorns();

            // Increase Gems counter
            // NumAcorn++;
            // Update Gems counter UI Text
            // textAcornUI.text = NumAcorn.ToString();

            Destroy(gameObject,2f);
        }
    }        
}
