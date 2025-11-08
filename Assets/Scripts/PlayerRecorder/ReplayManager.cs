using UnityEngine;
using static GhostPathsEnum;

public class ReplayManager : MonoBehaviour
{
    private PlayerRecorder recorder;
    private PlayerPlayback playback;

    [SerializeField] private bool startRecording;
    [SerializeField] private bool stopRecording;
    [SerializeField] private bool startPlayback;
    [SerializeField] private GhostPaths choosenPath = GhostPaths.WallJumpingPath;    
    private bool stopPlayback;

    private void Awake()
    {
        SaveManager.Init();
    }
    public void GetPlayerRefs(PlayerRecorder pr, PlayerPlayback pb)
    {
        recorder = pr;
        playback = pb;
    }
    public void CleanPlayerRefs()
    {
        recorder = null;
        playback = null;
    }
    void Update()
    {
        if (recorder == null || playback == null)
        {
            Debug.LogWarning("The "+ recorder.name + " and/or the " + playback.name + 
                            " refs. were not found on the Scene!!");
            return;
        }

        // Triggered by myself manually

        if (startRecording && !stopRecording && !startPlayback && !stopPlayback &&
            !recorder.IsRecording)
        {
            // Reset the StartRecordingFlag
            startRecording = false;

            recorder.StartRecording();
            Debug.Log("Comenzó la grabación.");
        }

        // Stopped by myself manually

        if (!startRecording && stopRecording && !startPlayback && !stopPlayback &&
            recorder.IsRecording)
        {
            // Reset the StopRecordingFlag
            stopRecording = false;

            recorder.StopRecording();
            Debug.Log("Grabación detenida.");
        }

        // The StartPlayback will be triggered from an Event
        // The same Event also should call playerMovement.DisableDamage();       
        // When the Event will finish (the ghost finish its seq) --> playerMovement.EnableDamage();       

        if (!startRecording && !stopRecording && startPlayback && !stopPlayback &&
            !playback.IsPlaying)
        {
            // Reset the StartPlaybackFlag
            startPlayback = false;

            //playback.StartPlayback(recorder.RecordedFrames, recorder.InitPos);
            playback.StartPlaybackFromJSON(choosenPath);
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

    #region Trigger Methods
    public void StartPlayback(GhostPaths pathToFollow)
    {
        choosenPath = pathToFollow;
        startPlayback = true;
    }
    #endregion
}
