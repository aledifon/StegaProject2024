using UnityEngine;

[System.Serializable]
public class PlayerFrameData
{
    public Vector2 position;
    public Vector2 velocity;
    public bool isGrounded;
    public bool isTouchingWall;
    public bool isJumping;
    public float time;
}
