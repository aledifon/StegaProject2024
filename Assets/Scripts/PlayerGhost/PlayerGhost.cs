using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    private List<PlayerFrameData> recordedFrames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    public void LoadFrames(List<PlayerFrameData> frames)
    {
        recordedFrames = frames;
        currentFrame = 0;
        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying || recordedFrames == null || currentFrame >= recordedFrames.Count)
            return;

        var frame = recordedFrames[currentFrame];

        transform.position = frame.inputX;

        currentFrame++;
    }

    public void StopPlayback()
    {
        isPlaying = false;
    }
}
