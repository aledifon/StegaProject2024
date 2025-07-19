using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerGhost : MonoBehaviour
{
    private List<PlayerFrameData> recordedFrames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    private PlayerGhostMovement playerGhostMovement;

    private Vector3 initGhostPos;

    private void Awake()
    {
        playerGhostMovement = GetComponent<PlayerGhostMovement>();
    }
    public void StartPlayback(List<PlayerFrameData> frames, Vector3 initPos)
    {
        recordedFrames = frames;
        initGhostPos = initPos;
        currentFrame = 0;
        isPlaying = true;        
    }

    void FixedUpdate()
    {
        if (!isPlaying || recordedFrames == null || currentFrame >= recordedFrames.Count)
            return;

        var frame = recordedFrames[currentFrame];

        if (currentFrame == 0)
            playerGhostMovement.transform.position = initGhostPos;

        playerGhostMovement.RbRecordedVelocity = frame.rbVelocity;
        playerGhostMovement.InputX = frame.inputX;
        playerGhostMovement.JumpPressed = frame.jumpPressed;
        playerGhostMovement.HookActionPressed = frame.hookActionPressed;

        currentFrame++;
    }

    public void StopPlayback()
    {
        isPlaying = false;
    }
}
