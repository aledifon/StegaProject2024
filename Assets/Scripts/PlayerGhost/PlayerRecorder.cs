using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public List<PlayerFrameData> RecordedFrames { get; private set; } = new();
    private Rigidbody2D rb;
    private bool isRecording = false;
    public bool IsRecording => isRecording;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isRecording) return;

        var frame = new PlayerFrameData
        {
            position = transform.position,
            velocity = rb.linearVelocity,
            time = Time.time
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
