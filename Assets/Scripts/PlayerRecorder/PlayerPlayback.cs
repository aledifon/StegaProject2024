using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class PlayerPlayback : MonoBehaviour
{
    //private List<PlayerFrameData> recordedFrames;
    private PlayerFramesData recordedFrames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    private PlayerGhostMovement playerGhostMovement;

    private Vector3 initGhostPos;

    private void Awake()
    {
        playerGhostMovement = GetComponent<PlayerGhostMovement>();
    }     
    void FixedUpdate()
    {
        if (!isPlaying || recordedFrames == null || currentFrame >= recordedFrames.frames.Count)
            return;

        var frame = recordedFrames.frames[currentFrame];

        if (currentFrame == 0 && initGhostPos != Vector3.zero)
            playerGhostMovement.transform.position = initGhostPos;

        playerGhostMovement.RbRecordedVelocity = frame.rbVelocity;
        playerGhostMovement.InputX = frame.inputX;
        playerGhostMovement.JumpPressed = frame.jumpPressed;
        playerGhostMovement.HookActionPressed = frame.hookActionPressed;

        currentFrame++;

        // When all the frames have been played then the playback is stopped.
        if (currentFrame == recordedFrames.frames.Count)
            StopPlayback();
    }
    public void StartPlaybackFromJSON(Vector3 initPos)
    {        
        // Load the Player Path data
        string playerPathStringData = SaveManager.Load();
        // Transform from JSON format Data to SaveObject Data
        if (playerPathStringData != null)
        {
            recordedFrames = JsonUtility.FromJson<PlayerFramesData>(playerPathStringData);
            //initGhostPos = Vector3.zero;
            initGhostPos = initPos;
            currentFrame = 0;            
            isPlaying = true;
        }
        Debug.Log("Load Player Path from the JSON File");
    }
    public void StartPlayback(PlayerFramesData frames, Vector3 initPos)
    {
        recordedFrames = frames;
        initGhostPos = initPos;
        currentFrame = 0;
        isPlaying = true;
    }
    public void StopPlayback()
    {
        recordedFrames = null;
        isPlaying = false;
    }
}
