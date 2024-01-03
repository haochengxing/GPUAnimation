using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachNode 
{
    public Alias AttachAlias;
    public Transform Node;
    public List<GameObject> AppendObjs;
    public AttachAnimationMetadata CurrentAttachMetadata;
    public AttachNode(Alias attachAlias, Transform node)
    {
        AttachAlias = attachAlias;
        Node = node;
        AppendObjs = new List<GameObject>(10);
    }

    public void Sync(int frame)
    {
        if (CurrentAttachMetadata==null)
        {
            Node.transform.localPosition = Vector3.zero;
            Node.transform.localRotation = Quaternion.identity;
        }
        else
        {
            if (CurrentAttachMetadata.Positions!=null)
            {
                Node.transform.localPosition = CurrentAttachMetadata.Positions[frame];
            }
            if (CurrentAttachMetadata.Rotations != null)
            {
                Node.transform.localRotation = CurrentAttachMetadata.Rotations[frame];
            }
        }
    }

    public void Attach(GameObject go)
    {
        AppendObjs.Add(go);
        go.transform.parent = Node;
    }

    public void Detach(GameObject go)
    {
        AppendObjs.Remove(go);
        go.transform.parent = null;
    }
}
