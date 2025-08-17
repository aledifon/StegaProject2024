using UnityEngine;

public class SpikesDetection : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float thrustToPlayer;
    [SerializeField] private bool isHorizontal;

    [Header("Damage")]
    [SerializeField] private int damageAmount;

    [SerializeField] private bool AreSpritesYFlipped;              // For isHorizontal = false:
                                                  // AreSpritesYFlipped = false --> Left oriented
                                                  // AreSpritesYFlipped = true --> Right oriented                                                  

                                                  // For isHorizontal = true:
                                                  // AreSpritesYFlipped = false --> Up oriented
                                                  // AreSpritesYFlipped = true --> Down oriented                                                  

    private void Awake()
    {
        SpriteRenderer spikesSprite;

        // Get the Sprites rotation on Z-Axis
        Transform spikesSpritesTransf = transform.parent.Find("SpikesSprites");
        if (spikesSpritesTransf != null)
        {
            isHorizontal = spikesSpritesTransf.localEulerAngles.z == 0;

            // Check if the Sprites are flipped or not on the Y-Axis        
            spikesSprite = spikesSpritesTransf.GetComponentInChildren<SpriteRenderer>();
            if (spikesSprite != null)
                AreSpritesYFlipped = spikesSprite.flipY;
            else
                Debug.LogError("There is no any child containing Sprite Renderer component");
        }            
        else
            Debug.LogError("There is no any child called SpikesSprites");        
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        // Set the Thrust direction
    //        Vector2 thrustDirection = Vector2.up;

    //        // Take Player's Damage & Disable the player's detection for a certain time            
    //        collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageAmount, thrustDirection, thrustToPlayer);
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Set the Thrust direction
            Vector2 thrustDirection;
            if (isHorizontal)                
                thrustDirection = AreSpritesYFlipped ? Vector2.down : Vector2.up;
            else
                thrustDirection = AreSpritesYFlipped ? Vector2.right : Vector2.left;

            // Take Player's Damage & Disable the player's detection for a certain time            
            collision.collider.GetComponent<PlayerHealth>().TakeDamage(damageAmount, thrustDirection, thrustToPlayer);
        }
    }
}
