using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public sealed class GPUSkinnedAnimationMetadata : AnimationMetadataBase
{
    public int StartIndex = 0;
}

[Serializable]
public sealed class GPUSkinnedAnimationMetadatas
{
    public Texture2D AnimationTexture;
    public int BoneCount = 0;
    public GPUSkinnedAnimationMetadata[] Metadatas;
}