using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialContainer : Counter
{
    public Material OriginMat;
    public Material Mat;
    public MaterialContainer(Material origin,Material mat)
    {
        OriginMat = origin;
        Mat = mat;
    }
    protected override void Clear()
    {
        if (Mat!=null)
        {
            Object.DestroyImmediate(Mat);
        }
    }
}
