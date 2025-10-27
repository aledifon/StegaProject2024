using UnityEngine;
using static UnityEngine.Object;
using UnityEngine.UI;
using DG.Tweening;

public static class CollectItemFlyFX
{
    public static void Play(Sprite sprite, Vector3 gameItemPos, RectTransform uiItem)
    {
        Canvas canvas = uiItem.GetComponentInParent<Canvas>();

        // Create a new Item GO, set it as child of the Canvas and add it the Item Sprite
        GameObject item = new GameObject("ItemToUI");
        //item.transform.SetParent(canvas.transform,false);

        Image img = item.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;

        // Get the Item RectTransform Ref.
        RectTransform itemRectTransform = item.GetComponent<RectTransform>();

        //Set as parent the same as the UI Item and also get the same pivots an anchors as the UI Item.
        itemRectTransform.SetParent(uiItem.parent,false);
        itemRectTransform.pivot = uiItem.pivot;
        itemRectTransform.anchorMin = uiItem.anchorMin;
        itemRectTransform.anchorMax = uiItem.anchorMax;
        // Set also the local scale and Size Delta (Width and Height)
        itemRectTransform.sizeDelta = uiItem.sizeDelta;
        itemRectTransform.localScale = uiItem.localScale + Vector3.one*0.2f;

        // Convert the Game Item Pos (World Pos) to Rect Transform (Canvas Pos) 
        // 1. Convert from GameItemPos(Vector3 on WorldPos) to ScreenPoint (Vector3 on Pixels)
        // 2. Convert from ScreenPoint (Vector3 on Pixels) to Rectangle Local Coord (Vector3 on UI coord.)
        Vector2 startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(    //2.
            //canvas.transform as RectTransform,
            uiItem.parent as RectTransform,
            Camera.main.WorldToScreenPoint(gameItemPos),             //1.
            null,
            out startPos
        );
        itemRectTransform.anchoredPosition = startPos;

        // The same process to get the Target Item Pos
        // (Needed to get the pos. on UI coord respect to the Canvas)        
        //Vector2 endPos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    //canvas.transform as RectTransform,
        //    uiItem.parent as RectTransform,
        //    Camera.main.WorldToScreenPoint(uiItem.position),             
        //    (canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null,
        //    out endPos
        //);
        // Alternative
        Vector2 endPos = uiItem.anchoredPosition;

        // Start the Tween Sequence
        Sequence seq = DOTween.Sequence();
        // Movement from start to target pos.
        seq.Append(itemRectTransform.DOAnchorPos(endPos, 0.5f).SetEase(Ease.InQuad));
        // Scale reduction
        //seq.Join(itemRectTransform.DOScale(uiItem.localScale, 0.8f).SetEase(Ease.Linear));
        // Delay
        seq.AppendInterval(0.2f);
        // Destroy the Item once tween is finished
        seq.AppendCallback(() => Destroy(item));             
    }
}
