using System.Runtime.CompilerServices;
using UnityEngine;

public class TriggerForwarderSpider : MonoBehaviour
{
    private string colliderId;
    private Spider spider;

    private void Awake()
    {
        spider = transform.parent.GetComponent<Spider>();
        colliderId = gameObject.name;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        spider.OnChildTriggerEnter(colliderId, collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        spider.OnChildTriggerExit(colliderId, collision);
    }
}
