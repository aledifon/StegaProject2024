using UnityEngine;

public class CamTriggerArea : MonoBehaviour
{
    Collider2D colliderTrigger;
    //PlayerMovement player;
    CameraFollow cameraFollow;

    [Header("Camera Settings")]
    [SerializeField] bool isExitCamArea;
    [SerializeField] CamOffsetMaskEnum.CamOffsetMask camOffsetMask;

    [SerializeField] float xCamOffset;
    [SerializeField] float yCamOffset;
    [SerializeField] float zCamOffset;

    [SerializeField] float sizeCam;
    [SerializeField] float sizeSmoothTime;

    private void Awake()
    {
        colliderTrigger = GetComponent<Collider2D>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Acorn dissappear
            colliderTrigger.enabled = false;

            //player = collision.GetComponent<PlayerMovement>();
            //if (player == null)
            //    Debug.LogError("PlayerMovement component not found on Player GO");

            if (isExitCamArea)
                cameraFollow.SetCameraDefSettings(sizeSmoothTime);
            else
                cameraFollow.SetCameraSettings(camOffsetMask,
                                            xCamOffset,
                                            yCamOffset,
                                            zCamOffset,
                                            sizeCam,
                                            sizeSmoothTime);
        }
    }

}
