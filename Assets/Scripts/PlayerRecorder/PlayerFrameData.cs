using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerFrameData
{
    public Vector2 rbVelocity;
    //public Vector3 initPos;    
    public float inputX;
    public bool jumpPressed;
    public bool hookActionPressed;
    public bool facingRight;

    // ?? Para reproducir las animaciones
    public int animStateHash;
    public float animNormalizedTime;
}

[System.Serializable]
public class PlayerFramesData
{
    public Vector3 initPosition;
    public List<PlayerFrameData> frames;    

    public PlayerFramesData()
    {
        initPosition = Vector3.zero;
        frames = new List<PlayerFrameData>();        
    }
}