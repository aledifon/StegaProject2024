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
}

[System.Serializable]
public class PlayerFramesData
{
    public List<PlayerFrameData> frames;

    public PlayerFramesData()
    {
        frames = new List<PlayerFrameData>();
    }
}