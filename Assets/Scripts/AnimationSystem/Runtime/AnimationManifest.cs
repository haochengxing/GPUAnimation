using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManifest : Counter
{
    public string Identity;
    public Dictionary<string, AnimationMetadataBase> AnimationMetadataDict;

    public AnimationManifest(string identity, Dictionary<string, AnimationMetadataBase> dict)
    {
        Identity = identity;
        AnimationMetadataDict = dict;
    }
}
