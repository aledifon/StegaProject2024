using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public List<PlayerFrameData> RecordedFrames { get; private set; } = new();
    private Rigidbody2D rb;
    private bool isRecording = false;
    public bool IsRecording => isRecording;

    private PlayerMovement playerMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isRecording) return;

        var frame = new PlayerFrameData
        {
            inputX = playerMovement.InputX,
            jumpPressed = playerMovement.JumpPressed,
            hookActionPressed = playerMovement.HookActionPressed
        };

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
