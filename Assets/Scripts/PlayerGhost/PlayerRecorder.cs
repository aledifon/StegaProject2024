using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public List<PlayerFrameData> RecordedFrames { get; private set; } = new();    
    private bool isRecording = false;
    public bool IsRecording => isRecording;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb2D;

    private Vector3 initPos;
    public Vector3 InitPos => initPos;

    void Awake()
    {        
        playerMovement = GetComponent<PlayerMovement>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!isRecording) return;

        var frame = new PlayerFrameData
        {
            rbVelocity = rb2D.linearVelocity,
            //initPos = playerMovement.transform.position,
            inputX = playerMovement.InputX,
            jumpPressed = playerMovement.JumpPressed,
            hookActionPressed = playerMovement.HookActionPressed
        };

        if (RecordedFrames.Count == 0)
            initPos = playerMovement.transform.position;

        RecordedFrames.Add(frame);               
    }

    public void StartRecording()
    {
        RecordedFrames.Clear();        
        isRecording = true;        
    }

    public void StopRecording()
    {
        isRecording = false;
    }
}
