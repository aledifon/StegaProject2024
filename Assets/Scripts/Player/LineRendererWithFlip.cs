using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class LineRendererWithFlip : MonoBehaviour
{
    // Flags vars.
    private bool flipSpriteDetected;

    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // GO components
    private LineRenderer lineRenderer;
    private Transform transformGrappingHook;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();  // Get LineRenderer component from Grapping-Hook GO        
        spriteRenderer = GetComponent<SpriteRenderer>();        // Get the Sprite Renderer componente from the player GO        
        transformGrappingHook = transform.Find("GrappingHook"); // Get Transform component from the Grapping-Hook GO      TO CHANGE THIS IN ORDER TO FIND THE Hook

        // Assure that the child called GrappingHook exist
        if (transformGrappingHook == null)
        {        
            Debug.LogError("It could not be found the 'GrappingHook' GO child on the player.");
        }

    }

    private void Start()
    {        
        // Define the line initial points
        Vector3 startPoint = new Vector3(0, 0, 0);              // Line origin
        Vector3 endPointRight = new Vector3(2.121f, 2.121f, 0); // 45º to the right
        Vector3 endPointLeft = new Vector3(-2.121f, 2.121f, 0); // 45º to the left

        // Set the LineRenderer Positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);                // Starting point 
        lineRenderer.SetPosition(1, endPointRight);             // Ending point (Initially to the right)

        // Configura el ancho de la línea
        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;

        // Asegúrate de que la línea esté en el plano 2D (z = 0)
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, new Vector3(endPointRight.x, endPointRight.y, 0));
    }

    // Update is called once per frame
    void Update()
    {        
        // Trigger the flip Sprite Detected flag when the player's direction changes
        if (spriteRenderer.flipX && (flipDirection == 1) ||
            !spriteRenderer.flipX && (flipDirection == -1))
        {
            flipSpriteDetected = true;                      // Updates the flag
            flipDirection = spriteRenderer.flipX ? -1 : 1;  // Updates the the player's flip direction var
        }
            
        // If the player's direction changes then the Line Renderer is also flipped
        if (flipSpriteDetected)
        {
            flipSpriteDetected = false;     // Reset the flip Sprite Detected flag
            FlipLineRenderer();
        }
    }

    void FlipLineRenderer()
    {
        // Ajusta la dirección de la línea
        Vector3 direction = new Vector3(2.121f * flipDirection, 2.121f, 0);  // 45º hacia la izquierda o derecha
        lineRenderer.SetPosition(1, transformGrappingHook.position + direction);  // Calcula el punto final de la línea
    }
}
