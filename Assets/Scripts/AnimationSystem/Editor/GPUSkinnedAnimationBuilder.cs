using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GPUSkinnedAnimationBuilder : AnimationBuilderBase
{
    public const string AnimationTexturePathFormat = "{0}/{1}_Animation.asset";
    public const string GPUSkinnedMeshPathFormat = "{0}/{1}.asset";
    public const int AnimationStartOffset = 4;
    private Texture2D mTexture;
    private GPUSkinnedAnimationMetadatas mGPUSkinnedAnimationMetadatas;
    private Dictionary<int, List<Color>> mColors = new Dictionary<int, List<Color>>();
    public GPUSkinnedAnimationBuilder(AnimationRecord record) : base(record)
    {
        int width = 128;
        int height = 128;
        AnimationBuilderUtil.GetAnimationTextureSize(record.Animations, BoneHierarchicals.Length, record.AnimationFramePreSecond, AnimationStartOffset, out width, out height);
        mTexture = new Texture2D(width,height,TextureFormat.RGBAHalf,false);
        mTexture.filterMode = FilterMode.Point;
        mTexture.wrapMode = TextureWrapMode.Clamp;
        mGPUSkinnedAnimationMetadatas = new GPUSkinnedAnimationMetadatas();
        mGPUSkinnedAnimationMetadatas.BoneCount = BoneHierarchicals.Length;
        mGPUSkinnedAnimationMetadatas.AnimationTexture = mTexture;
        mGPUSkinnedAnimationMetadatas.Metadatas = new GPUSkinnedAnimationMetadata[AnimationClipIndices.Length];
        Dictionary<AnimationClip, int> animationOffset = new Dictionary<AnimationClip, int>();
        int startIndex = AnimationStartOffset;
        for (int i = 0; i < AnimationClipIndices.Length; i++)
        {
            int clipIndex = AnimationClipIndices[i];
            AnimationContainer alias = record.Animations[clipIndex];
            AnimationClip animation = alias.Animation;
            GPUSkinnedAnimationMetadata skinnedMetadata = AnimationBuilderUtil.SetAnimationMetadata<GPUSkinnedAnimationMetadata>(animation,record.AnimationFramePreSecond);
            int offset = startIndex;
            if (animationOffset.ContainsKey(animation))
            {
                offset = animationOffset[animation];
            }
            else
            {
                animationOffset.Add(animation,offset);
                startIndex += skinnedMetadata.TotalFrame;
            }
            skinnedMetadata.StartIndex = offset;
            mGPUSkinnedAnimationMetadatas.Metadatas[i] = skinnedMetadata;
        }
    }
    protected override void Update(int animationIndex)
    {
        List<Color> colors;
        if (!mColors.TryGetValue(animationIndex,out colors))
        {
            colors = new List<Color>();
            mColors.Add(animationIndex, colors);
        }
        for (int i = 0; i < BoneHierarchicals.Length; i++)
        {
            Transform bone = ModelObj.transform.Find(BoneHierarchicals[i]);
            Matrix4x4 bindPose = AnimationBuilderUtil.GetBoneBindPose(bone, SkinnedRenderers);
            Matrix4x4 animMat = bone.localToWorldMatrix * bindPose;
            Color c = Color.black;
            c.r = animMat.m00;
            c.g = animMat.m01;
            c.b = animMat.m02;
            c.a = animMat.m03;
            colors.Add(c);
            c.r = animMat.m10;
            c.g = animMat.m11;
            c.b = animMat.m12;
            c.a = animMat.m13;
            colors.Add(c);
            c.r = animMat.m20;
            c.g = animMat.m21;
            c.b = animMat.m22;
            c.a = animMat.m23;
            colors.Add(c);
        }
    }
    protected override void onSave()
    {
        Dictionary<string, Mesh> createMeshDict = new Dictionary<string, Mesh>();
        string[] boneTrans = AnimationBuilderUtil.GetAllBone(ModelObj);
        for (int i = 0; i < SkinnedRenderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = SkinnedRenderers[i];
            Mesh orgin = renderer.sharedMesh;
            Mesh mesh = AnimationBuilderUtil.DepthCopyMesh(orgin);
            List<Vector4> boneIndexes;
            List<Vector4> boneWeights;
            AnimationBuilderUtil.ExtractBoneInfos(orgin,renderer,boneTrans,out boneIndexes,out boneWeights);
            mesh.SetUVs(2, boneIndexes);
            mesh.SetUVs(3, boneWeights);
            MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Low);
            mesh.UploadMeshData(mesh);
            AssetDatabase.CreateAsset(mesh,string.Format(GPUSkinnedMeshPathFormat,SaveDirectory,orgin.name));
            createMeshDict.Add(orgin.name,mesh);
        }
        Color[] colorBuffer = mTexture.GetPixels();
        int bufferIndex = AnimationStartOffset * 3 * mGPUSkinnedAnimationMetadatas.BoneCount;
        foreach (var item in mColors)
        {
            for (int j = 0; j < item.Value.Count; j++)
            {
                colorBuffer[bufferIndex++] = item.Value[j];
            }
        }
        mTexture.SetPixels(colorBuffer);
        mTexture.Apply(false,true);
        string animPath = string.Format(AnimationTexturePathFormat, SaveDirectory, Record.Model.name);
        AssetDatabase.CreateAsset(mTexture, animPath);
        GameObject gpuSkinnedPrefab = GameObjectTools.Instantiate(null, null, Record.Model.name, Vector3.zero, Quaternion.identity, Vector3.one);
        gpuSkinnedPrefab.layer = Record.Layer;
        if (AttachManifest!=null)
        {
            for (int i = 0; i < AttachManifest.AttachMetadatas.Length; i++)
            {
                AttachMetadata attach = AttachManifest.AttachMetadatas[i];
                string alias = attach.AttachAlias.AliasName;
                GameObjectTools.Instantiate(null, gpuSkinnedPrefab.transform, alias, Vector3.zero, Quaternion.identity, Vector3.one);
            }
        }
        MeshRenderer[] renderers = new MeshRenderer[Record.Materials.Length];
        int index = 0;
        foreach (var item in Record.Materials)
        {
            Transform node = ModelObj.transform.Find(item.NodeName);
            Mesh mesh = null;
            for (int i = 0; i < SkinnedRenderers.Length; i++)
            {
                if (SkinnedRenderers[i].transform==node)
                {
                    if (!createMeshDict.TryGetValue(SkinnedRenderers[i].sharedMesh.name,out mesh))
                    {
                        continue;
                    }
                    break;
                }
            }
            if (mesh==null)
            {
                continue;
            }
            GameObject go = new GameObject(mesh.name);
            go.layer = Record.Layer;
            go.transform.parent = gpuSkinnedPrefab.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = item.NodeMaterial;
            item.NodeMaterial.enableInstancing = true;
            item.NodeMaterial.SetTexture(GPUSkinnedAnimationManager.ShaderProperty.GPUSkinnedAnimationTex, mTexture);
            item.NodeMaterial.SetFloat(GPUSkinnedAnimationManager.ShaderProperty.GPUSkinnedPixelsPerFrame, mGPUSkinnedAnimationMetadatas.BoneCount*3f);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            renderers[index] = renderer;
            index++;
        }
        GPUSkinnedAnimationRenderer gpuAnimationRenderer = gpuSkinnedPrefab.AddComponent<GPUSkinnedAnimationRenderer>();
        gpuAnimationRenderer.AnimationMetadatas = mGPUSkinnedAnimationMetadatas;
        gpuAnimationRenderer.Renderers = renderers;
        AnimationRenderer = gpuAnimationRenderer;
    }
}
