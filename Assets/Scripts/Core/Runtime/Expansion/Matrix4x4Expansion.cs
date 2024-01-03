using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Matrix4x4Expansion
{
    public static Vector3 GetPosition(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.m03,matrix.m13,matrix.m23);
    }

    public static Quaternion GetRotation(this Matrix4x4 matrix)
    {
        return matrix.rotation;
    }
}
