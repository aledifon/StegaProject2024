using System.Runtime.CompilerServices;
using UnityEngine;

public class TriggerForwarderPlant : MonoBehaviour
{
    private string colliderId;
    private PlantCarnivore plantCarnivore;

    private void Awake()
    {
        plantCarnivore = transform.parent.GetComponent<PlantCarnivore>();
        colliderId = gameObject.name;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        plantCarnivore.OnChildTriggerEnter(colliderId, collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        plantCarnivore.OnChildTriggerExit(colliderId, collision);
    }
}
