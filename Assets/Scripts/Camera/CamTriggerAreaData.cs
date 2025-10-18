using UnityEngine;

[System.Serializable]
public class CamTriggerAreaData
{    
    public CamTriggerAreaEnum.CamTriggerArea camTriggerAreaId;  // Area Id
    public CamBoundariesTriggerArea respawnCamBoundTriggerArea; // Cinemachin Confiner Ref.
    public Transform respawnPos;                                // Respawn Pos.
}
