using UnityEngine;
using UnityEngine.UIElements;

public class CamBoundariesTriggerArea : MonoBehaviour
{
    private Collider2D boundsCollider;    
    public Collider2D BoundsCollider => boundsCollider;
    private Collider2D regionDetectionCollider;
    private CameraFollow cameraFollow;        

    private void Awake()
    {
        // Get Refs.        
        regionDetectionCollider = GetComponent<Collider2D>();
        if (regionDetectionCollider == null)
            Debug.LogError("The region Detection Collider Not Found on this gameobject " + gameObject);
        boundsCollider = transform.Find("BoundsCollider").GetComponent<Collider2D>();
        if (boundsCollider == null)
            Debug.LogError("The BoundsCollider Not Found as child of this gameobject " + gameObject);

        cameraFollow = FindAnyObjectByType<CameraFollow>();
        //cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow == null)            
            Debug.LogError("CameraFollow component Not Found on the Scene!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (cameraFollow != null && cameraFollow.ConfinerTriggersEnabled)
                cameraFollow.SetTargetBoundaries(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (cameraFollow != null && cameraFollow.ConfinerTriggersEnabled)
                cameraFollow.ClearTargetBoundaries(this);
        }
    }
    public Vector4 GetBounds()
    {
        Vector3 min;
        Vector3 max;

        // Get the Bounds coullider boundaries        
        min = boundsCollider.bounds.min;
        max = boundsCollider.bounds.max;

        return new Vector4(min.x, max.x, min.y, max.y);
    }
}
