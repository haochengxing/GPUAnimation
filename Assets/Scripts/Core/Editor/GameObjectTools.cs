using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTools
{
    public static string GetHierarchical(Transform node,bool includeTop)
    {
        if (node == null)
        {
            return string.Empty;
        }
        string hierarchical = string.Empty;
        if (node.parent != null)
        {
            string parent = GetHierarchical(node.parent, includeTop);
            hierarchical = string.IsNullOrEmpty(parent) ? node.name : parent + "/" + node.name;
        }
        else if (includeTop)
        {
            hierarchical = node.name;
        }
        return hierarchical;
    }

    public static GameObject Instantiate(GameObject origin,Transform parent,string name,Vector3 position,Quaternion rotation,Vector3 scale)
    {
        GameObject go = null;
        if (origin == null)
        {
            go = new GameObject();
        }
        else
        {
            go = GameObject.Instantiate(origin);
        }
        go.name = name;
        go.transform.parent = parent;
        go.transform.localPosition = position;
        go.transform.localRotation = rotation;
        go.transform.localScale = scale;

        return go;
    }
}
