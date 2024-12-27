using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class HookManager : MonoBehaviour
{    
    // Determines the player's direction
    float flipDirection = 1;                    // 1 = Player looks right ; -1 = Player looks left

    // GO components
    private LineRenderer lineRenderer;    
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private CapsuleCollider2D hookCollider2D;

    // Start is called before the first frame update
    void Awake()
    {
        // Get the Hook Capsule Collider 2D Component        
        hookCollider2D = GetComponent<CapsuleCollider2D>();

        // Get LineRenderer component from the Rope GO                
        lineRenderer = transform.parent?.Find("Rope")?.GetComponent<LineRenderer>();
        // Get the Sprite Renderer component from the player
        spriteRenderer = GameObject.FindWithTag("Player")?.GetComponent<SpriteRenderer>();        
        // Get the Player Movement component (script) from the player
        playerMovement = GameObject.FindWithTag("Player")?.GetComponent<PlayerMovement>();   
    }    

    // Update is called once per frame
    void Update()
    {                       
        flipDirection = spriteRenderer.flipX ? -1 : 1;  // Updates the the player's flip direction var every frame
        
        if (playerMovement.CanEnableHook)
        {
            playerMovement.EnableHookToFalse();          // Reset the Hook flag
            StartCoroutine(nameof(EnableGrapplingHook));   // Shows the LineRenderer of the Rope on its correct direction
                                                         // + Enables and set the offset of the CapsuleCollider2D of the Hook
        }
        
        if (playerMovement.CurrentJumpState == PlayerMovement.JumpingState.Swinging)
            UpdateLineRenderer();
    }
    // Updates the Line Renderer when the player is on the Swinging State
    void UpdateLineRenderer()
    {
        // Here will be updated the Rope (Line Renderer) angle respect to the player
    }
    // Enables the Grappling Hook elements
    IEnumerator EnableGrapplingHook()
    {
        // Define the line initial points
        Vector3 startPoint = new Vector3(0, 0, 0);                              // Line origin
        Vector3 endPointRight = new Vector3(2.121f * flipDirection, 2.121f, 0); // 45º to the right or to the left

        // Set the LineRenderer Positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);                // Starting point 
        lineRenderer.SetPosition(1, endPointRight);             // Ending point (Initially to the right)                

        // Configura el ancho de la línea
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Configure the Hook offsets & Enable his CapsuleCollider2D
        hookCollider2D.offset = endPointRight;
        hookCollider2D.enabled = true;

        yield return new WaitForSeconds(2);                     // Leave the Line Renderer visible for 2s
        
        // Hides The Line Renderer if the player is not on the Swinging State after 2s
        if (playerMovement.CurrentJumpState != PlayerMovement.JumpingState.Swinging)
            DisableGrapplingHook();
    }
    // Disable the Grappling Hook elements
    void DisableGrapplingHook()
    {
        // Hides the Rope (Line Renderer)
        lineRenderer.positionCount = 0;
        // Disables the Hook (Circle Collider 2D)
        hookCollider2D.enabled = false;
    }

    // Collisions Detections with the Grappling Points
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }
}
