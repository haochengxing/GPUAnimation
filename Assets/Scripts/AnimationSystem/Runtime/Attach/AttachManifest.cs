using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachManifest : ScriptableObject
{
    public AttachMetadata[] AttachMetadatas;
    public Dictionary<string, AttachMetadata> AttachMetadataDict;
    public void Init()
    {
        if (AttachMetadatas==null)
        {
            return;
        }
        if (AttachMetadataDict!=null)
        {
            return;
        }
        AttachMetadataDict = new Dictionary<string, AttachMetadata>();
        for (int i = 0; i < AttachMetadatas.Length; i++)
        {
            AttachMetadata metadata = AttachMetadatas[i];
            if (metadata==null)
            {
                continue;
            }
            metadata.Init();
            if (AttachMetadataDict.ContainsKey(metadata.AttachAlias.AliasName))
            {
                continue;
            }
            AttachMetadataDict.Add(metadata.AttachAlias.AliasName, metadata);
        }
    }
}
