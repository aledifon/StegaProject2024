using UnityEngine;

public class LavaDetection : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float thrustToPlayer;

    [Header("Damage")]
    [SerializeField] private int damageAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Set the Thrust direction
            Vector2 thrustDirection = Vector2.up;

            // Take Player's Damage & Disable the player's detection for a certain time            
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageAmount, thrustDirection, thrustToPlayer);
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.CompareTag("Player"))
    //    {
    //        // Set the Thrust direction
    //        Vector2 thrustDirection = Vector2.up;

    //        // Take Player's Damage & Disable the player's detection for a certain time            
    //        collision.collider.GetComponent<PlayerHealth>().TakeDamage(damageAmount, thrustDirection, thrustToPlayer);            
    //    }
    //}
}
