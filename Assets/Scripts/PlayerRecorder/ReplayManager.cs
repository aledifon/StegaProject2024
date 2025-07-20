using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public PlayerRecorder recorder;
    public PlayerPlayback playback;

    public bool startRecording;
    public bool stopRecording;
    public bool startPlayback;
    public bool stopPlayback;

    private void Awake()
    {
        SaveManager.Init();
    }

    void Update()
    {
        // Triggered by myself manually

        if (startRecording && !stopRecording && !startPlayback && ! stopPlayback &&
            !recorder.IsRecording)
        {
            recorder.StartRecording();
            Debug.Log("Comenzó la grabación.");
        }

        // Stopped by myself manually

        if (!startRecording && stopRecording && !startPlayback && !stopPlayback &&
            recorder.IsRecording)
        {
            recorder.StopRecording();
            Debug.Log("Grabación detenida.");
        }

        // The StartPlayback will be triggered from an Event
        // The same Event also should call playerMovement.DisableDamage();       
        // When the Event will finish (the ghost finish its seq) --> playerMovement.EnableDamage();       

        if (!startRecording && !stopRecording && startPlayback && !stopPlayback &&
            !playback.IsPlaying)
        {
            //playback.StartPlayback(recorder.RecordedFrames, recorder.InitPos);
            playback.StartPlaybackFromJSON(recorder.InitPos);
            Debug.Log("Reproducción iniciada.");
        }

        // The StopPlayback will be triggered internally from PlayerPlayback.FixedUpdate()

        //if (!startRecording && !stopRecording && !startPlayback && stopPlayback &&
        //    playback.IsPlaying)
        //{
        //    playback.StopPlayback();
        //    Debug.Log("Reproducción detenida.");
        //}
    }
}
