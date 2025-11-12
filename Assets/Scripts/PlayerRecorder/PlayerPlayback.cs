using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

using static GhostPathsEnum;

public class PlayerPlayback : MonoBehaviour
{
    //private List<PlayerFrameData> recordedFrames;
    private PlayerFramesData recordedFrames;
    private int currentFrame = 0;
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    private PlayerGhostMovement playerGhostMovement;    

    //private Vector3 initGhostPos;

    //private int frameHoldCounter = 0;
    //public int slowFactor = 2;

    private void Awake()
    {
        playerGhostMovement = GetComponent<PlayerGhostMovement>();                 
    }     
    void FixedUpdate()
    {
        if (!isPlaying || recordedFrames.frames == null || currentFrame >= recordedFrames.frames.Count)
            return;

        var frame = recordedFrames.frames[currentFrame];

        if (currentFrame == 0 && recordedFrames.initPosition != Vector3.zero)
            playerGhostMovement.transform.position = recordedFrames.initPosition;

        // ?? Replay the recorded animation
        playerGhostMovement.Animator_.speed = 0.5f; // normal, o menor si quieres ralentizar
        //playerGhostMovement.Animator_.Play(frame.animStateHash, 0, frame.animNormalizedTime);
        playerGhostMovement.Animator_.Update(Time.fixedDeltaTime);

        playerGhostMovement.RbRecordedVelocity = frame.rbVelocity;
        playerGhostMovement.InputX = frame.inputX;
        playerGhostMovement.JumpPressed = frame.jumpPressed;
        playerGhostMovement.HookActionPressed = frame.hookActionPressed;
        playerGhostMovement.SpriteRendPlayer.flipX = !frame.facingRight;

        currentFrame++;

        // Alternative way in order to slow down the playback
        //frameHoldCounter++;
        //if(frameHoldCounter >= slowFactor)
        //{
        //    frameHoldCounter = 0;
        //    currentFrame++;
        //}

        // When all the frames have been played then the playback is stopped.
        if (currentFrame == recordedFrames.frames.Count)
            StopPlayback();
    }    
    public void StartPlaybackFromJSON(GhostPaths path)
    {
        // Load the Player Path data        
        string playerPathStringData;

#if UNITY_WEBGL && !UNITY_EDITOR        
    // WebGL --> Coroutine
        StartCoroutine(SaveManager.LoadAsync(path, (playerPathStringData) =>
        {
            if (playerPathStringData != null)
            {
                recordedFrames = JsonUtility.FromJson<PlayerFramesData>(playerPathStringData);
                currentFrame = 0;
                isPlaying = true;
                Debug.Log("Load Player Path from JSON (WebGL)");
            }
            else
            {
                Debug.LogWarning("The JSON could'nt be loaded on WebGL");
            }

        }));  
#else
        // PC / Editor -> Transform from JSON format Data to SaveObject Data
        playerPathStringData = SaveManager.Load(path);        
        if (playerPathStringData != null)
        {
            recordedFrames = JsonUtility.FromJson<PlayerFramesData>(playerPathStringData);
            //initGhostPos = Vector3.zero;
            //initGhostPos = initPos;
            currentFrame = 0;
            isPlaying = true;
        }
        Debug.Log("Load Player Path from the JSON File (Desktop)");
#endif        
    }
    public void StartPlayback(PlayerFramesData frames, Vector3 initPos)
    {
        recordedFrames = frames;
        recordedFrames.initPosition = initPos;
        currentFrame = 0;
        isPlaying = true;
    }
    public void StopPlayback()
    {
        playerGhostMovement.transform.position = playerGhostMovement.WaitingPos.position;
        recordedFrames = null;
        isPlaying = false;
    }    
}
