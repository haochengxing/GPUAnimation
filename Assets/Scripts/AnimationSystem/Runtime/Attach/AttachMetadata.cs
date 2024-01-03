using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AttachMetadata 
{
    public Alias AttachAlias;
    public AttachAnimationMetadata[] AnimationMetadatas;
    public Dictionary<string, AttachAnimationMetadata> AnimationMetadataDict;

    public void Init()
    {
        if (AnimationMetadatas==null)
        {
            return;
        }
        AnimationMetadataDict = new Dictionary<string, AttachAnimationMetadata>(AnimationMetadatas.Length);
        for (int i = 0; i < AnimationMetadatas.Length; i++)
        {
            AttachAnimationMetadata metadata = AnimationMetadatas[i];
            if (metadata==null)
            {
                continue;
            }
            if (metadata.Positions==null&&metadata.Rotations==null)
            {
                continue;
            }
            if (AnimationMetadataDict.ContainsKey(metadata.AnimationName))
            {
                continue;
            }
            AnimationMetadataDict.Add(metadata.AnimationName, metadata);
        }
    }
}
