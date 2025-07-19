using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public PlayerRecorder recorder;
    public PlayerGhost ghost;

    public bool startRecording;
    public bool stopRecording;
    public bool startPlayback;
    public bool stopPlayback;

    void Update()
    {
        if (startRecording && !stopRecording && !startPlayback && ! stopPlayback &&
            !recorder.IsRecording)
        {
            recorder.StartRecording();
            Debug.Log("Comenzó la grabación.");
        }

        if (!startRecording && stopRecording && !startPlayback && !stopPlayback &&
            recorder.IsRecording)
        {
            recorder.StopRecording();
            Debug.Log("Grabación detenida.");
        }

        if (!startRecording && !stopRecording && startPlayback && !stopPlayback &&
            !ghost.IsPlaying)
        {
            ghost.StartPlayback(recorder.RecordedFrames, recorder.InitPos);
            Debug.Log("Reproducción iniciada.");
        }

        if (!startRecording && !stopRecording && !startPlayback && stopPlayback &&
            ghost.IsPlaying)
        {
            ghost.StopPlayback();
            Debug.Log("Reproducción detenida.");
        }
    }
}
