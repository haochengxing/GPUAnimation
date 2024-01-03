using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBoundsManifest : ScriptableObject
{
    public AnimationBoundsMetadata[] AnimationBoundsMetadatas = null;
    public Dictionary<string, AnimationBoundsMetadata> AnimationBoundsMetadataDict = null;
    public void Init()
    {
        if (AnimationBoundsMetadatas==null)
        {
            return;
        }
        if (AnimationBoundsMetadataDict != null)
        {
            return;
        }
        AnimationBoundsMetadataDict = new Dictionary<string, AnimationBoundsMetadata>(AnimationBoundsMetadatas.Length);
        for (int i = 0; i < AnimationBoundsMetadatas.Length; i++)
        {
            AnimationBoundsMetadata metadata = AnimationBoundsMetadatas[i];
            if (AnimationBoundsMetadataDict.ContainsKey(metadata.AnimationName))
            {
                continue;
            }
            AnimationBoundsMetadataDict.Add(metadata.AnimationName, metadata);
        }
    }
}
