using UnityEngine;

public class Canvas : MonoBehaviour
{    
    private void OnEnable()
    {
        GameManager.Instance.GetCanvasRef(gameObject);
    }
}
