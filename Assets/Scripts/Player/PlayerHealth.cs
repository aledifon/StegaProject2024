using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    public float recoveryHealth;

    [Header("UI")]
    //public Image acornUI;

    [Header("Death")]
    public float forceJumpDeath;
    //public GameManager gameManager;

    Animator anim;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
        //acornUI.fillAmount = 1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    //Public method which I'll call from the enemy's script
    public void TakeDamage(int amount)
    {
        //In case hurting anim. is running or the player is death then we'll return.
        if (anim.GetBool("Hurt") || currentHealth <= 0) return; 

        currentHealth -= amount;
        //acornUI.fillAmount = currentHealth / maxHealth;

        anim.SetBool("Hurt", true);
        //stop the player, reset the speed
        //playerMovement.ResetVelocity();

        if (currentHealth <= 0)
        {
            Death();
            return;
        }
        Invoke("HurtToFalse", 1);
    }
    void HurtToFalse()
    {
        anim.SetBool("Hurt", false);
    }
    void Death()
    {
        //Call the GameOver method of the GameManager script
        //gameManager.GameOver();
        //When the player dies disable his collider, add +Y Force and increase x2 his size
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().AddForce(Vector2.up * forceJumpDeath);
        //GetComponent<Rigidbody2D>().gravityScale = 10;        
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(2, 2, 2));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Acorn"))
        {
            currentHealth += recoveryHealth;
            //Checking current health is always lower or equal than maxHealth
            //if (currentHealth > maxHealth) currentHealth = maxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            //acornUI.fillAmount = currentHealth / maxHealth;               
            Destroy(collision.gameObject);
        }
    }
}
