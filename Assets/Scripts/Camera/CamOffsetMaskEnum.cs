using UnityEngine;

public class CamOffsetMaskEnum
{
    [System.Flags]
    public enum CamOffsetMask
    {
        None = 0,
        X = 1 << 0,     // d = 1 | b = 0001
        Y = 1 << 1,     // d = 2 | b = 0010
        Z = 1 << 2,     // d = 4 | b = 0100  
        Size = 1 << 3   // d = 8 | b = 1000  
    }
}
