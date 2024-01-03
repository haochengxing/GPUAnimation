using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationRendererBase : MonoBehaviour
{
    private static Dictionary<string, AnimationManifest> mManifestDict = new Dictionary<string, AnimationManifest>(40);
    private static MaterialPropertyBlock mPropertyBlock = null;

    public MaterialPropertyBlock PropertyBlock
    {
        get
        {
            if (mPropertyBlock == null)
            {
                mPropertyBlock = new MaterialPropertyBlock();
            }
            return mPropertyBlock;
        }
    }

    private AnimationManifest mManifest;
    public bool ForceVisible = true;
    public Renderer[] Renderers;
    public bool Visible
    {
        get
        {
            bool visible = true;
            if (Renderers != null)
            {
                for (int i = 0; i < Renderers.Length; i++)
                {
                    Renderer renderer = Renderers[i];
                    if (renderer != null && !renderer.isVisible)
                    {
                        visible = false;
                        break;
                    }
                }
            }
            else
            {
                visible = false;
            }
            return ForceVisible && visible;
        }
    }
    protected string Identity = string.Empty;
    protected int InstanceId = -1;
    public virtual bool Init(string identity, int instanceId)
    {
        Identity = identity;
        InstanceId = instanceId;
        return mManifestDict.TryGetValue(identity, out mManifest);
    }
    public virtual AnimationMetadataBase PlayAnimation(string animationName) { throw new NotImplementedException(); }
    public virtual void PlayAnimationFrame(int frame) { throw new NotImplementedException(); }
    public virtual AnimationMetadataBase GetCurrentAnimationMetadata() { throw new NotImplementedException(); }
    public virtual void Release() { }
    public virtual void SetPlaySpeed(float speed) { }
    public int GetAnimationFrameCount(string animationName)
    {
        AnimationMetadataBase metadata = GetAnimationMetadataByName(animationName);
        if (metadata == null)
        {
            return -1;
        }
        return metadata.TotalFrame;
    }
    public AnimationMetadataBase GetAnimationMetadataByName(string animationName)
    {
        if (mManifest.AnimationMetadataDict==null)
        {
            return null;
        }
        AnimationMetadataBase metadata = null;
        if (!mManifest.AnimationMetadataDict.TryGetValue(animationName,out metadata))
        {
            return null;
        }
        return metadata;
    }
    protected void SetAnimationManifest(string identity,Dictionary<string,AnimationMetadataBase> animationMetadataDict)
    {
        if (mManifestDict.ContainsKey(identity))
        {
            mManifest = mManifestDict[identity];
            return;
        }
        mManifest = new AnimationManifest(identity,animationMetadataDict);
        mManifestDict.Add(identity, mManifest);
    }
    private void OnDestroy()
    {
        if (mManifest==null)
        {
            return;
        }
        if (mManifest.Release())
        {
            mManifestDict.Remove(mManifest.Identity);
            mManifest = null;
        }
    }

    public virtual void SetColor(int propertyId,Color color,bool share)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (share)
            {
                renderer.sharedMaterial.SetColor(propertyId,color);
            }
            else
            {
                renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetColor(propertyId,color);
                renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
    public void SetColor(string propertyName, Color color, bool share)
    {
        SetColor(Shader.PropertyToID(propertyName),color,share);
    }
    public virtual void SetFloat(int propertyId, float value, bool share)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (share)
            {
                renderer.sharedMaterial.SetFloat(propertyId, value);
            }
            else
            {
                renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetFloat(propertyId, value);
                renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
    public void SetFloat(string propertyName, float value, bool share)
    {
        SetFloat(Shader.PropertyToID(propertyName), value, share);
    }
    public virtual void SetMatrix(int propertyId, Matrix4x4 matrix, bool share)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (share)
            {
                renderer.sharedMaterial.SetMatrix(propertyId, matrix);
            }
            else
            {
                renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetMatrix(propertyId, matrix);
                renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
    public void SetMatrix(string propertyName, Matrix4x4 matrix, bool share)
    {
        SetMatrix(Shader.PropertyToID(propertyName), matrix, share);
    }
    public virtual void SetTexture(int propertyId, Texture texture, bool share)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (share)
            {
                renderer.sharedMaterial.SetTexture(propertyId, texture);
            }
            else
            {
                renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetTexture(propertyId, texture);
                renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
    public void SetTexture(string propertyName, Texture texture, bool share)
    {
        SetTexture(Shader.PropertyToID(propertyName), texture, share);
    }
    public virtual void SetVector(int propertyId, Vector4 vector, bool share)
    {
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderer renderer = Renderers[i];
            if (share)
            {
                renderer.sharedMaterial.SetVector(propertyId, vector);
            }
            else
            {
                renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetVector(propertyId, vector);
                renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
    public void SetVector(string propertyName, Vector4 vector, bool share)
    {
        SetVector(Shader.PropertyToID(propertyName), vector, share);
    }
}
