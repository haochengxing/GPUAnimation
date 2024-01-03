using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUSkinnedAnimationManager : Singleton<GPUSkinnedAnimationManager>
{
    public const int GPUSkinnedMaterialQueueStartAt = 2100;
    public static class ShaderProperty
    {
        public static readonly int GPUSkinnedFrame = Shader.PropertyToID("_FrameIndex");
        public static readonly int GPUSkinnedAnimationTex = Shader.PropertyToID("_AnimationTex");
        public static readonly int GPUSkinnedPixelsPerFrame = Shader.PropertyToID("_NumPixelsPerFrame");
    }
    public Dictionary<Material, MaterialContainer> MaterialMap = new Dictionary<Material, MaterialContainer>();
    private List<string> mMaterialQueue = new List<string>(512);
    public GPUSkinnedAnimationManager() { }
    private static bool mInit = false;
    private static bool mInstancing = true;
    public static bool Instancing
    {
        get
        {
            if (!mInit)
            {
#if !UNITY_EDITOR
                mInstancing = !SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3.0");
#endif
                mInit = true;

            }
            return mInstancing;
        }
    }
    public Material GetMaterial(Material origin)
    {
        if (origin==null)
        {
            return null;
        }
        Material material = new Material(origin);
        enableInstancing(material,false);
        return material;
    }

    private void enableInstancing(Material material,bool enable)
    {
        if (material==null)
        {
            return;
        }
        material.enableInstancing = enable;
    }
    public int GetMaterialQueue(string identity)
    {
        if (!Instancing)
        {
            return (int)RenderQueue.Geometry;
        }
        int queue = mMaterialQueue.IndexOf(identity);
        if (queue<0)
        {
            mMaterialQueue.Add(identity);
            queue = mMaterialQueue.Count;
        }
        return GPUSkinnedMaterialQueueStartAt + queue;
    }
}
