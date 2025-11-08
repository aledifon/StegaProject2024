using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;

public class PlayerGhostMovement : PlayerMovement
{   
    private PlayerPlayback playerGhost;

    private bool jumpWasPressed;
    private bool hookActionWasPressed;
    
    private Collider2D playerCollider;

    private Vector2 rbRecordedVelocity;
    public Vector2 RbRecordedVelocity
    {
        get => rbRecordedVelocity;
        set => rbRecordedVelocity = value;
    }

    public new float InputX
    {
        get => inputX;
        set => inputX = value;
    }
    public new bool JumpPressed
    {
        get => jumpPressed;
        set => jumpPressed = value;
    }
    public new bool HookActionPressed
    {
        get => hookActionPressed;
        set => hookActionPressed = value;
    }

    [Header("WaitingPos")]
    [SerializeField] private Transform waitingPos;
    public Transform WaitingPos => waitingPos;

    #region Unity API    
    protected override void OnDrawGizmos()
    {
        // Skip this method from the base class
    }
    protected override void OnEnable()
    {
        // Skip this method from the base class
    }
    protected override void OnDisable()
    {
        // Skip this method from the base class
    }
    protected override void Awake()
    {
        GetGORefs();
        playerGhost = GetComponent<PlayerPlayback>();

        if (waitingPos == null)
            Debug.LogError("No Waiting Position was assigned to the PlayerGhost");

        // Get the player Collider
        playerCollider = GameObject.Find("Player")?.GetComponent<Collider2D>();
        if (playerCollider == null)
            Debug.LogError("No Collider2D Component was found on the Player GO or the" +
                        "the Player GO was not found on the scene");
        else
            SkipPlayerCollisions();
    }
    protected override void Update()
    {
        base.Update();        
    }
    protected override void FixedUpdate()
    {        
        base.FixedUpdate();

        JumpActionInputGhost();
        HookActionInputGhost();
        MoveActionInputGhost();

        if (playerGhost.IsPlaying)
            rb2D.linearVelocity = rbRecordedVelocity;
    }    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // Skip this method from the base class
    }
    #endregion
    #region Input Player      
    public override void JumpActionInput(InputAction.CallbackContext context)
    {
        // Skip this method from the base class
    }
    public override void HookActionInput(InputAction.CallbackContext context)
    {
        // Skip this method from the base class
    }
    public override void MoveActionInput(InputAction.CallbackContext context)
    {
        // Skip this method from the base class
    }
    private void JumpActionInputGhost()
    {
        if (jumpPressed && !jumpWasPressed)
        {
            // Enable the Jump Buffer Timer            
            SetJumpBufferTimer();
        }
        jumpWasPressed = jumpPressed;
    }
    private void HookActionInputGhost()
    {
        if (hookActionPressed && !hookActionWasPressed)
        {
            // Enable the Hook timer
            if (IsJumping && IsHookUnlocked)
                SetHookThrownTimer();
        }
        hookActionWasPressed = hookActionPressed;
    }
    private void MoveActionInputGhost()
    {
        // Flip the player sprite & change the animations State
        FlipSprite(InputX, inputDirDeadZone);
        //AnimatingRunning(inputX);
    }
    #endregion
    //#region InitPlayerGhost
    //public void MoveToWaitingPosition()
    //{
    //    transform.position = waitingPos.position;
    //}
    //#endregion
    #region Collisions
    private void SkipPlayerCollisions()
    {
        if (boxCollider2D != null && playerCollider != null)
        {
            // Ignora colisiones entre el PlayerGhost y el Player
            Physics2D.IgnoreCollision(boxCollider2D, playerCollider, true);
        }
        else
        {
            Debug.LogWarning("Colliders not assigned or found!");
        }
    }
    #endregion
}
