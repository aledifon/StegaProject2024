using System.Runtime.CompilerServices;
using UnityEngine;

public class TriggerForwarderBat : MonoBehaviour
{
    private string colliderId;
    private Bat bat;

    private void Awake()
    {
        bat = transform.parent.GetComponent<Bat>();
        colliderId = gameObject.name;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bat.OnChildTriggerEnter(colliderId, collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        bat.OnChildTriggerExit(colliderId, collision);
    }
}
