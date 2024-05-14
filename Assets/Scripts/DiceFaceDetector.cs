using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceFaceDetector : MonoBehaviour
{
    [field: SerializeField] 
    public DiceFace DiceFace { get; private set; }
    public float GetYPositionGlobal()
    {
        return transform.position.y;
    }
}

public enum DiceFace
{
    Rabbit,
    Sheep,
    Pig,
    Cow,
    Horse,
    Wolf,
    Fox,
}
