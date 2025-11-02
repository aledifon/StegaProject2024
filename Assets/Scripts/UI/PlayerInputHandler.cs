using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerMovement playerMovement;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();

        var actions = playerInput.actions;
        actions["Jump"].performed += playerMovement.JumpActionInput;
        actions["Jump"].canceled += playerMovement.JumpActionInput;
        actions["Hook"].performed += playerMovement.HookActionInput;
        actions["Hook"].canceled += playerMovement.HookActionInput;

        var moveAction = playerInput.actions["Move"];
        moveAction.started += playerMovement.MoveActionInput;
        moveAction.performed += playerMovement.MoveActionInput;
        moveAction.canceled += playerMovement.MoveActionInput;        

        actions["Pause"].performed += GameManager.Instance.PauseResumeGameInput;
        actions["Resume"].performed += GameManager.Instance.PauseResumeGameInput;
        actions["QuitInGame"].performed += GameManager.Instance.QuitGameUI;

        GameManager.Instance.GetPlayerInputRefs(playerInput);
    }
    private void OnDisable()
    {
        var actions = playerInput.actions;
        actions["Jump"].performed -= playerMovement.JumpActionInput;
        actions["Jump"].canceled -= playerMovement.JumpActionInput;
        actions["Hook"].performed -= playerMovement.HookActionInput;
        actions["Hook"].canceled -= playerMovement.HookActionInput;

        var moveAction = playerInput.actions["Move"];
        moveAction.started -= playerMovement.MoveActionInput;
        moveAction.performed -= playerMovement.MoveActionInput;
        moveAction.canceled -= playerMovement.MoveActionInput;

        if (GameManager.Instance != null)
        {
            actions["Pause"].performed -= GameManager.Instance.PauseResumeGameInput;
            actions["Resume"].performed -= GameManager.Instance.PauseResumeGameInput;
            actions["QuitInGame"].performed -= GameManager.Instance.QuitGameUI;
        }
    }
}
