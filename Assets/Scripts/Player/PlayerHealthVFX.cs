using UnityEngine;
using static UnityEngine.Object;
using UnityEngine.UI;
using DG.Tweening;

public static class PlayerHealthVFX
{
    public static void Play(RectTransform rectTransform, 
        float duration = 0.5f, float strength = 10f, int vibrato = 10, float randomness = 90f,
        float targetScale = 1f, float originScale = 2f)
    {   
        // Cancel any previous Tween active on this object
        rectTransform.DOKill();

        // Restore the original State of the Health Icon
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one * originScale;

        // Calculate the quarterDuration for the Scale Yo-Yo VFX
        float quarterDuration = duration / 4;

        // Start the Tween Sequence
        Sequence seq = DOTween.Sequence();

        // Shaking FX
        seq.Append(rectTransform.DOShakeAnchorPos(
            duration,   // Shaking Total duration
            strength,   // Max. pixel diplacement
            vibrato,    // Amount of oscilations
            randomness, // Movement variability (0 = lineal, 180 = high randomness)
            true       // snapping: If you want it to fit to whole pixels
        ).SetUpdate(true));

        // Scaling In-Out FX
        seq.Join(rectTransform.DOScale(targetScale, quarterDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(4, LoopType.Yoyo)
            .SetUpdate(true));        
        seq.OnComplete(() => {
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one * originScale;
        });

        seq.Play();        
    }
}
