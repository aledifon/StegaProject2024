using UnityEngine;
using UnityEngine.UIElements;

public class CamBoundariesTriggerArea : MonoBehaviour
{
    Collider2D boundsCollider;    
    Collider2D regionCollider;    
    CameraFollow cameraFollow;        

    private void Awake()
    {
        // Get Refs.        
        regionCollider = GetComponent<Collider2D>();
        boundsCollider = transform.Find("BoundsCollider").GetComponent<Collider2D>();

        cameraFollow = Camera.main.GetComponent<CameraFollow>();        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {                                          
            if(cameraFollow != null)
                cameraFollow.SetTargetBoundaries(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {                                          
            if(cameraFollow != null)
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
