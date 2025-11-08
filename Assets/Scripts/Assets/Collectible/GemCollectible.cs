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
            if (collision.gameObject.name != "Player")
                return;

            isCaptured = true;

            // Save the sprite before disabling it
            Sprite gemsprite = spriteRenderer.sprite;

            myCollider.enabled = false;
            spriteRenderer.enabled = false;            

            audioSource.PlayOneShot(clip);

            // Increase Gems counter
            collision.gameObject.GetComponent<PlayerMovement>().IncreaseGems();

            // Play Gem Fly VFX
            CollectItemFlyFX.Play(gemsprite, transform.position,GameManager.Instance.GemsUIImage);

            Destroy(gameObject,2f);
        }
    }        
}
