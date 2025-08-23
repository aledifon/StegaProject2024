using UnityEngine;

public class RatAttack : MonoBehaviour
{    
    Rat rat;

    private void Awake()
    {        
        rat = transform.parent.GetComponent<Rat>();

        if (rat == null)
            Debug.LogError("Was not found the Rat componente on the parent of " + gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {            
            rat.Attack();
        }        
    }    
}
