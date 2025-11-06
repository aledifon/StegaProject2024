using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class ColumnsDestructionHandler : MonoBehaviour
{
    [SerializeField] List<GEOVolatile> columns;
    [SerializeField] ElevatorMovement platformAccesWallJumpArea;
    private Transform accesWallJumpAreaPos;
    public Transform AccesWallJumpAreaPos => accesWallJumpAreaPos;

    private void Awake()
    {
        if (columns == null || columns.Any(x => x == null))
            Debug.LogError("No List of GEOVolatile components were added on the Inspector " +
                            "or any of the elements of the list is null");

        if (platformAccesWallJumpArea == null)
            Debug.LogError("No Elevator movement component was added on the Inspector");
        else
            accesWallJumpAreaPos = platformAccesWallJumpArea.transform;
    }
    //private void OnEnable()
    //{
    //    CameraFollow cameraFollow = FindAnyObjectByType<CameraFollow>();
    //    if (cameraFollow == null)
    //        Debug.LogError("No any CameraFollow Component was found on the scene");
    //    else
    //        cameraFollow.GetColumnsDestructHandlerRef(this);
    //}

    public void TriggerColumnsDestruction()
    {
        foreach (var column in columns)
            column.DestroyTrigger();
    }
    public IEnumerator TriggerWallJumpPlatformDestruction()
    {
        Tilemap tm = platformAccesWallJumpArea.GetComponentInChildren<Tilemap>();
        if(tm == null)
        {
            Debug.LogError("No Tilemap component was found on the GO " + platformAccesWallJumpArea.gameObject.name);
            yield break;
        }            
        else
        {
            Color targetColor = tm.color;
            targetColor.a = 0;

            yield return DOTween.To(
                () => tm.color,
                x => tm.color = x,
                targetColor,
                2f
            ).SetEase(Ease.InOutSine)
            .WaitForCompletion();

            Destroy(platformAccesWallJumpArea.gameObject);
        }                
    }    
}
