using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputHandler : MonoBehaviour
{
    PlayerInput playerInput;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();       
        
        var actions = playerInput.actions;        
        actions["PressKey"].performed += GameManager.Instance.KeyPressedUI;        
        actions["QuitMenu"].performed += GameManager.Instance.QuitGameUI;

        var navigateAction = playerInput.actions["Navigate"];
        navigateAction.started += GameManager.Instance.SwitchSelectionUI;
        navigateAction.performed += GameManager.Instance.SwitchSelectionUI;
        navigateAction.canceled += GameManager.Instance.SwitchSelectionUI;

        GameManager.Instance.GetPlayerInputRefs(playerInput);
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            var actions = playerInput.actions;
            actions["PressKey"].performed -= GameManager.Instance.KeyPressedUI;
            actions["QuitMenu"].performed -= GameManager.Instance.QuitGameUI;

            var navigateAction = playerInput.actions["Navigate"];
            navigateAction.started -= GameManager.Instance.SwitchSelectionUI;
            navigateAction.performed -= GameManager.Instance.SwitchSelectionUI;
            navigateAction.canceled -= GameManager.Instance.SwitchSelectionUI;
        }
    }
}
