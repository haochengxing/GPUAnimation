using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AnimationBuilderUtil 
{
    public const string AnimationRecordsPath = "Assets/Scripts/AnimationSystem/Editor/AnimationRecords.asset";
    public static AnimationRecords GetAnimationRecords()
    {
        AnimationRecords records = ScriptableObject.CreateInstance<AnimationRecords>();
        AnimationRecords orgin = AssetDatabase.LoadAssetAtPath<AnimationRecords>(AnimationRecordsPath);
        if (orgin==null)
        {
            return records;
        }
        records.Records = orgin.Records;
        records.Init();
        return records;
    }
    public static void ExtractBoneInfos(Mesh mesh,SkinnedMeshRenderer skinned,string[]boneTrans,out List<Vector4>boneIndexes,out List<Vector4>boneWeights)
    {
        boneIndexes = new List<Vector4>(mesh.vertexCount);
        boneWeights = new List<Vector4>(mesh.vertexCount);
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            BoneWeight boneWeight = mesh.boneWeights[i];
            int index0 = getIndex(skinned.bones[boneWeight.boneIndex0],boneTrans);
            int index1 = getIndex(skinned.bones[boneWeight.boneIndex1],boneTrans);
            int index2 = getIndex(skinned.bones[boneWeight.boneIndex2],boneTrans);
            int index3 = getIndex(skinned.bones[boneWeight.boneIndex3],boneTrans);
            boneIndexes.Add(new Vector4(index0,index1,index2,index3));
            boneWeights.Add(new Vector4(boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3));
        }
    }
    public static Mesh DepthCopyMesh(Mesh orgin)
    {
        if (orgin==null)
        {
            return null;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = orgin.vertices;
        for (int i = 0; i < orgin.subMeshCount; i++)
        {
            mesh.SetIndices(orgin.GetIndices(i),MeshTopology.Triangles,i);
        }
        mesh.colors = orgin.colors;
        mesh.normals = orgin.normals;
        mesh.tangents = orgin.tangents;
        mesh.triangles = orgin.triangles;
        mesh.uv = orgin.uv;
        mesh.uv2 = orgin.uv2;
        mesh.bounds = orgin.bounds;
        mesh.bindposes = orgin.bindposes;
        mesh.boneWeights = orgin.boneWeights;
        mesh.colors32 = orgin.colors32;
        mesh.indexFormat = orgin.indexFormat;
        mesh.name = orgin.name;
        return mesh;
    }
    private static int getIndex(Transform node,string[]nodes)
    {
        if (node==null)
        {
            return -1;
        }
        string hierarchical = GameObjectTools.GetHierarchical(node,false);
        return Array.IndexOf(nodes, hierarchical);
    }
    public static string [] GetAllBone(GameObject model)
    {
        List<string> boneTrans = new List<string>();
        Transform[] trans = model.GetComponentsInChildren<Transform>();
        for (int i = 0; i < trans.Length; i++)
        {
            Transform boneNode = trans[i];
            if (boneNode.gameObject.GetComponent<Renderer>()!=null)
            {
                continue;
            }
            if (boneNode.gameObject.GetComponent<Animator>() != null)
            {
                continue;
            }
            boneTrans.Add(GameObjectTools.GetHierarchical(boneNode,false));
        }
        boneTrans.Sort();
        return boneTrans.ToArray();
    }
    public static Matrix4x4 GetBoneBindPose(Transform bone,SkinnedMeshRenderer[]renderers)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            int index = Array.IndexOf(renderer.bones,bone);
            if (index<renderer.sharedMesh.bindposes.Length&&index>=0)
            {
                return renderer.sharedMesh.bindposes[index];
            }
        }
        return Matrix4x4.identity;
    }
    public static void GetAnimationTextureSize(AnimationContainer[] animations,int boneCount,int frameCountPreSecond,int offset,out int width,out int height)
    {
        List<AnimationClip> animationClips = new List<AnimationClip>();
        float frameCount = 0;
        for (int i = 0; i < animations.Length; i++)
        {
            if (animations[i].Animation==null)
            {
                continue;
            }
            AnimationClip animation = animations[i].Animation;
            if (animationClips.Contains(animation))
            {
                continue;
            }
            animationClips.Add(animation);
            frameCount += (frameCountPreSecond == -1 ? animation.frameRate : frameCountPreSecond) * animation.length;
        }
        float total = (offset + frameCount) * boneCount * 3;
        int w = Mathf.CeilToInt(Mathf.Sqrt(total));
        if (!Mathf.IsPowerOfTwo(w))
        {
            width = Mathf.NextPowerOfTwo(w);
        }
        else
        {
            width = w;
        }
        int h = Mathf.CeilToInt(total/(float)width);
        if (!Mathf.IsPowerOfTwo(h))
        {
            height = Mathf.NextPowerOfTwo(h);
        }
        else
        {
            height = h;
        }
    }
    public static Mesh BakeMesh(SkinnedMeshRenderer renderer)
    {
        Mesh mesh = new Mesh();
        renderer.BakeMesh(mesh);
        Vector3[] vertices = mesh.vertices;
        Vector3[] target = new Vector3[vertices.Length];
        Quaternion rotation = renderer.transform.rotation;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 dis = rotation * vertex;
            target[i] = dis + rotation * renderer.transform.position;
            target[i].x = -target[i].x;
        }
        mesh.vertices = target;
        mesh.RecalculateNormals();
        MeshUtility.Optimize(mesh);
        return mesh;
    }
    public static T SetAnimationMetadata<T>(AnimationClip animation,int frameCountPreSecond) where T : AnimationMetadataBase, new()
    {
        if (animation==null)
        {
            return null;
        }
        T metadata = new T();
        metadata.AnimationName = animation.name;
        float rate = frameCountPreSecond == -1 ? animation.frameRate : frameCountPreSecond;
        metadata.FrameRate = (int)rate;
        metadata.FrameStep = 1f / rate;
        metadata.Length = animation.length;
        metadata.Loop = animation.isLooping;
        metadata.TotalFrame = Mathf.FloorToInt(animation.length*rate);
        return metadata;
    }
    public static int[] GetAnimationClipCount(AnimationContainer[] animations)
    {
        List<AnimationClip> clips = new List<AnimationClip>();
        List<int> clipsIndex = new List<int>();
        for (int i = 0; i < animations.Length; i++)
        {
            AnimationContainer alias = animations[i];
            if (alias==null||alias.Animation==null)
            {
                continue;
            }
            if (!clips.Contains(alias.Animation))
            {
                clips.Add(alias.Animation);
                clipsIndex.Add(i);
            }
        }
        return clipsIndex.ToArray();
    }
}
