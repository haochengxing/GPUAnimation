using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AnimationBoundsMetadata
{
    public string AnimationName = string.Empty;
    public Bounds ModelBounds = new Bounds(Vector3.zero,Vector3.zero);
}
