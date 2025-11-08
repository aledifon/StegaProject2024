using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    //public List<PlayerFrameData> RecordedFrames { get; private set; } = new List<PlayerFrameData>();    
    public PlayerFramesData RecordedFrames { get; private set; } = new PlayerFramesData();
    private bool isRecording = false;
    public bool IsRecording => isRecording;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb2D;

    //private Vector3 initPos;
    //public Vector3 InitPos => initPos;

    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        var stateInfo = playerMovement.Animator_.GetCurrentAnimatorStateInfo(0);

        var frame = new PlayerFrameData
        {
            rbVelocity = rb2D.linearVelocity,
            //initPos = playerMovement.transform.position,
            inputX = playerMovement.InputX,
            jumpPressed = playerMovement.JumpPressed,
            hookActionPressed = playerMovement.HookActionPressed,
            facingRight = (playerMovement.SpriteRendPlayerFlipX == false),

            animStateHash = stateInfo.shortNameHash,
            animNormalizedTime = stateInfo.normalizedTime %1f
        };

        if (RecordedFrames.frames.Count == 0)
            //initPos = playerMovement.transform.position;
            RecordedFrames.initPosition = playerMovement.transform.position;

        // Save the Player Data on every frame
        RecordedFrames.frames.Add(frame);        
    }

    public void StartRecording()
    {
        RecordedFrames.frames.Clear();        
        isRecording = true;        
    }

    public void StopRecording()
    {
        // Transform the SaveObject Data to JSON format & Save it on a file
        string json = JsonUtility.ToJson(RecordedFrames);
        SaveManager.Save(json);
        Debug.Log("Saved Player Path to the JSON File");

        // Stop the recording
        isRecording = false;
    }
}
