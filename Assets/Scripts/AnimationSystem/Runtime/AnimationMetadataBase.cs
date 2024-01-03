using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AnimationMetadataBase 
{
    public string AnimationName = string.Empty;
    public bool Loop = true;
    public int TotalFrame = 30;
    public int FrameRate = 30;
    public float Length = 1f;
    public float FrameStep = 0.03333333f;
}
