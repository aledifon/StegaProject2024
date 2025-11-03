using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;  
using static UIMenuSelectEnum;

public class MenuSelector : MonoBehaviour
{
    [SerializeField] private List<RectTransform> optionsTextContainer;
    [SerializeField] private RectTransform selector;
    private Vector3 offsetSelector;

    private void Awake()
    {
        // Set the Offset Selector
        offsetSelector = new Vector3(0, -23f, 0);

        // Set the Selector on the initPos (Start Game Pos)        
        // Get the 'Start Game' Pos. on World Pos. coords
        Vector2 startGameWorldpos = optionsTextContainer[(int)UIMenuSelect.StartGame].position;
        // Get the 'Start Game' Pos. on Local Pos. coords respect to 'MenuPanel'
        Vector3 startGameLocalPos = selector.parent.InverseTransformPoint(startGameWorldpos);

        // Keep the relative X local pos. on the the Selector respect to the Menu Panel
        startGameLocalPos.x = selector.anchoredPosition.x;
        // Update only the relative Y local pos. on the the Selector respect to the Menu Panel
        selector.localPosition = startGameLocalPos;

        // Correct the Selector pos.
        selector.localPosition += offsetSelector;
    }
    public void UpdateSelectorPos(UIMenuSelect menuSelection)
    {
        //float targetPosY = optionsTextContainer[(int)menuSelection].anchoredPosition.y;

        //selector.DOAnchorPosY(targetPosY, 0.2f)
        //        .SetEase(Ease.OutQuad);

        // Get the World Pos of the target Pos of the Selector
        Vector2 worldTargetPos = optionsTextContainer[(int)menuSelection].position;
        // Convert from world to local Pos (on the MenuPanel Scope)
        Vector3 localTargetPos = selector.parent.InverseTransformPoint(worldTargetPos);

        // Correct the Selector pos.
        localTargetPos += offsetSelector;

        // Update only the Y local pos. on the Selector (without modifying the X Local Pos.)
        localTargetPos.x = selector.anchoredPosition.x;        
        selector.DOAnchorPosY(localTargetPos.y, 0.2f)
                .SetEase(Ease.OutQuad);
    }
}
