using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUSkinnedAnimationRenderer : AnimationRendererBase
{
    public GPUSkinnedAnimationMetadatas AnimationMetadatas;
    private GPUSkinnedAnimationMetadata mCurrentMetadata;
    [HideInInspector]
    public int CurrentAnimationFrame = 0;
    [HideInInspector]
    public int CullingIndex = -1;
    public override bool Init(string identity, int instanceId)
    {
        if( !base.Init(identity, instanceId))
        {
            Dictionary<string, AnimationMetadataBase> animationMetadataDict = null;
            if (AnimationMetadatas!=null)
            {
                animationMetadataDict = new Dictionary<string, AnimationMetadataBase>(AnimationMetadatas.Metadatas.Length);
                for (int i = 0; i < AnimationMetadatas.Metadatas.Length; i++)
                {
                    GPUSkinnedAnimationMetadata metadata = AnimationMetadatas.Metadatas[i];
                    if (animationMetadataDict.ContainsKey(metadata.AnimationName))
                    {
                        continue;
                    }
                    animationMetadataDict.Add(metadata.AnimationName, metadata);
                }
            }
            base.SetAnimationManifest(identity, animationMetadataDict);
        }
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (renderer==null)
            {
                continue;
            }
            int queue = GPUSkinnedAnimationManager.Instance.GetMaterialQueue(identity+renderer.name);
#if UNITY_EDITOR
            Material mat = UnityEditor.EditorApplication.isPlaying ? renderer.material : null;
#else
            Material mat = renderer.sharedMaterial;
#endif
            if (mat==null)
            {
                continue;
            }
            mat.renderQueue = queue;
            mat.enableInstancing = GPUSkinnedAnimationManager.Instancing;
        }
        return true;
    }
    public override AnimationMetadataBase PlayAnimation(string animationName)
    {
        AnimationMetadataBase metadata = GetAnimationMetadataByName(animationName);
        if (metadata==null)
        {
            return null;
        }
        mCurrentMetadata = metadata as GPUSkinnedAnimationMetadata;
        CurrentAnimationFrame = 0;
        return mCurrentMetadata;
    }
    public override void PlayAnimationFrame(int frame)
    {
        CurrentAnimationFrame = frame + mCurrentMetadata.StartIndex;
        SetFloat(GPUSkinnedAnimationManager.ShaderProperty.GPUSkinnedFrame,CurrentAnimationFrame,false);
    }
}
