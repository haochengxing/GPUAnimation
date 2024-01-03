using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public sealed class AnimationPlayer : MonoBehaviour
{
    public static readonly Bounds ZoneBounds = new Bounds(Vector3.zero,Vector3.zero);
    public static Dictionary<string, Dictionary<string, string>> AnimationAliasMap = new Dictionary<string, Dictionary<string, string>>();
    public delegate bool AnimationCallback(params int []args);
    [HideInInspector]
    public string Identity = string.Empty;
    public EAnimationType AnimationType = EAnimationType.SkinnedAnimation;
    public Alias[] AnimationAlias;
    public AttachManifest AttachManifest;
    public AnimationBoundsManifest AnimationBoundsManifest;
    public int CurrentFrame = 0;
    public string CurrentName = string.Empty;
    public AnimationRendererBase AnimationRenderer;
    private AnimationMetadataBase mCurrentAnimationMetadata;
    private float mTimer = 0f;
    private AnimationCallback mCallback;
    private int[] mCallbackParams;
    private bool mLoop = false;
    private bool mReverse = false;
    private float mSpeed = 1.0f;
    private bool mPause = false;
    private Dictionary<string, AttachNode> mAttach = new Dictionary<string, AttachNode>(10);
    private Dictionary<string, string> AliasMap;
    private void Awake()
    {
        if (string.IsNullOrEmpty(Identity))
        {
            return;
        }
        if (!AnimationAliasMap.TryGetValue(Identity,out AliasMap))
        {
            AliasMap = new Dictionary<string, string>();
            for (int i = 0; i < AnimationAlias.Length; i++)
            {
                Alias alias = AnimationAlias[i];
                if (!AliasMap.ContainsKey(alias.AliasName))
                {
                    AliasMap.Add(alias.AliasName,alias.Name);
                }
            }
            AnimationAliasMap.Add(Identity,AliasMap);
        }
        if (AnimationRenderer==null)
        {
            return;
        }
        AnimationRenderer.Init(Identity,gameObject.GetInstanceID());
        if (AttachManifest != null)
        {
            AttachManifest.Init();
            foreach (var item in AttachManifest.AttachMetadataDict)
            {
                Alias alias = item.Value.AttachAlias;
                if (mAttach.ContainsKey(alias.AliasName))
                {
                    continue;
                }
                string path = string.Empty;
                path = alias.AliasName;
                Transform node = AnimationRenderer.transform.Find(path);
                if (node==null)
                {
                    node = new GameObject(alias.AliasName).transform;
                    node.parent = AnimationRenderer.transform;
                    node.localPosition = Vector3.zero;
                    node.localScale = Vector3.one;
                    node.localRotation = Quaternion.identity;
                }
                AttachNode attachNode = new AttachNode(alias,node);
                mAttach.Add(alias.AliasName, attachNode);
            }
        }
        if (AnimationBoundsManifest==null)
        {

        }
        else
        {
            AnimationBoundsManifest.Init();
        }
        if (AnimationAlias==null || AnimationAlias.Length==0)
        {
            return;
        }
        Play(AnimationAlias[0].AliasName);
    }
    public int GetAnimationTypeInt() 
    {
        return (int)AnimationType;
    }
    public void Play(string animationAlias,bool loop=true,bool reverse=false,AnimationCallback callback=null,params int[]args)
    {
        setAnimationData(animationAlias,loop,reverse,-1,args,callback,0f);
    }
    public void Play(string animationName, bool loop = true, bool reverse = false)
    {
        if (!verify())
        {
            return;
        }
        AnimationCallback callback = null;
        Play(animationName,loop,reverse,callback);
    }
    public void Pause()
    {
        if (!verify())
        {
            return;
        }
        mPause = true;
    }
    public void Continue()
    {
        if (!verify())
        {
            return;
        }
        mPause = false;
    }
    public void PlayFrame(string animationAlias,int frame)
    {
        setAnimationData(animationAlias,false,false,frame,null,null,-1f);
    }
    public Bounds GetBounds()
    {
        if (AnimationBoundsManifest==null)
        {
            return ZoneBounds;
        }
        if (mCurrentAnimationMetadata==null)
        {
            return ZoneBounds;
        }
        AnimationBoundsMetadata metadata;
        if (!AnimationBoundsManifest.AnimationBoundsMetadataDict.TryGetValue(mCurrentAnimationMetadata.AnimationName,out metadata))
        {
            return ZoneBounds;
        }
        return metadata.ModelBounds;
    }
    public Bounds GetBounds(string animationAlias)
    {
        if (AnimationBoundsManifest == null)
        {
            return ZoneBounds;
        }
        string animationName = string.Empty;
        if (mCurrentAnimationMetadata == null)
        {
            return ZoneBounds;
        }
        AnimationBoundsMetadata metadata;
        if (!AnimationBoundsManifest.AnimationBoundsMetadataDict.TryGetValue(animationName, out metadata))
        {
            return ZoneBounds;
        }
        return metadata.ModelBounds;
    }
    public int GetFrameCount(string animationAlias)
    {
        string animationName = string.Empty;
        return AnimationRenderer.GetAnimationFrameCount(animationName);
    }
    public void Attach(string attachAlias,GameObject obj)
    {
        if (obj==null)
        {
            return;
        }
        if (AttachManifest==null)
        {
            return;
        }
        AttachNode attachNode = getAttachNode(attachAlias);
        if (attachNode==null)
        {
            return;
        }
        attachNode.Attach(obj);
    }
    public void Detach(string attachAlias, GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        if (AttachManifest == null)
        {
            return;
        }
        AttachNode attachNode = getAttachNode(attachAlias);
        if (attachNode == null)
        {
            return;
        }
        attachNode.Detach(obj);
    }
    public void GetAttachPosition(string attachAlias, out Vector3 position,out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        AttachNode attachNode = getAttachNode(attachAlias);
        if (attachNode == null)
        {
            return;
        }

        Matrix4x4 matrix = attachNode.Node.localToWorldMatrix;
        position = matrix.GetPosition();
        rotation = matrix.GetRotation();
    }
    public string[] GetAllAnimationName()
    {
        string[] names = new string[AnimationAlias.Length];
        for (int i = 0; i < AnimationAlias.Length; i++)
        {
            names[i] = AnimationAlias[i].AliasName;
        }
        return names;
    }
    public void SetVisible(bool visible)
    {
        if (!verify())
        {
            return;
        }
        AnimationRenderer.ForceVisible = visible;
    }
    public void SetPlaySpeed(float speed)
    {
        mSpeed = speed;
        if (!verify())
        {
            return;
        }
        AnimationRenderer.SetPlaySpeed(mSpeed);
    }
    private void setAnimationData(string animationAlias,bool loop,bool reverse,int currentFrame,int[]args,AnimationCallback callback,float timer)
    {
        if (!verify())
        {
            return;
        }
        string animationName = getAnimationNameByAlias(animationAlias);
        if (string.IsNullOrEmpty(animationName))
        {
            return;
        }
        mCurrentAnimationMetadata = AnimationRenderer.PlayAnimation(animationName);
        if (mCurrentAnimationMetadata==null)
        {
            return;
        }
        if (currentFrame<0)
        {
            currentFrame = mReverse ? mCurrentAnimationMetadata.TotalFrame - 1 : 0;
        }
        CurrentName = animationAlias;
        mLoop = loop;
        mReverse = reverse;
        CurrentFrame = currentFrame;
        mCallback = callback;
        mCallbackParams = args;
        mTimer = timer;
        syncAttachData();
        AnimationRenderer.PlayAnimationFrame(currentFrame);
        syncAttach(currentFrame);
    }
    private void syncAttachData()
    {
        if (AttachManifest==null)
        {
            return;
        }
        foreach (var item in mAttach)
        {
            item.Value.CurrentAttachMetadata = null;
            if (AttachManifest.AttachMetadataDict!=null)
            {
                AttachMetadata metadata;
                if (AttachManifest.AttachMetadataDict.TryGetValue(item.Value.AttachAlias.AliasName,out metadata))
                {
                    if (metadata.AnimationMetadataDict!=null)
                    {
                        AttachAnimationMetadata animationMetadata;
                        if (metadata.AnimationMetadataDict.TryGetValue(mCurrentAnimationMetadata.AnimationName,out animationMetadata))
                        {
                            item.Value.CurrentAttachMetadata = animationMetadata;
                        }
                    }
                }
            }
        }
    }
    private void syncAttach(int frame)
    {
        if (AttachManifest==null)
        {
            return;
        }
        foreach (var item in mAttach)
        {
            if (item.Value.CurrentAttachMetadata==null)
            {
                syncAttachData();
            }
            item.Value.Sync(frame);
        }
    }
    private bool verify()
    {
        if (AnimationRenderer==null)
        {
            return false;
        }
        return true;
    }
    private AttachNode getAttachNode(string attachAlias)
    {
        AttachNode attachNode;
        if (!mAttach.TryGetValue(attachAlias,out attachNode))
        {

        }
        return attachNode;
    }
    private string getAnimationNameByAlias(string alias)
    {
        if (!AliasMap.ContainsKey(alias))
        {
            return string.Empty;
        }
        return AliasMap[alias];
    }
    private void FixedUpdate()
    {
        if (AnimationRenderer == null)
        {
            return;
        }
        if (!AnimationRenderer.Visible)
        {
            return;
        }
        if (mCurrentAnimationMetadata == null)
        {
            return;
        }
        if (mTimer < 0)
        {
            return;
        }
        if (mPause)
        {
            return;
        }
        if ((!mLoop && !mReverse && CurrentFrame == mCurrentAnimationMetadata.TotalFrame - 1) || (!mLoop && mReverse && CurrentFrame == 0))
        {
            return;
        }
        mTimer += Time.fixedDeltaTime * mSpeed;
        if (mTimer<mCurrentAnimationMetadata.FrameStep)
        {
            return;
        }
        int frame = Mathf.FloorToInt(mTimer/mCurrentAnimationMetadata.FrameStep);
        mTimer -= mCurrentAnimationMetadata.FrameStep * frame;
        int nextFrame = 0;
        if (mLoop)
        {
            if (mReverse)
            {
                nextFrame = (CurrentFrame - frame) % mCurrentAnimationMetadata.TotalFrame;
                if (nextFrame<0)
                {
                    nextFrame = mCurrentAnimationMetadata.TotalFrame + nextFrame;
                }
            }
            else
            {
                nextFrame = (CurrentFrame + frame) % mCurrentAnimationMetadata.TotalFrame;
            }
        }
        else
        {
            if (mReverse)
            {
                nextFrame = CurrentFrame - frame;
            }
            else
            {
                nextFrame = CurrentFrame + frame;
            }
        }
        if (nextFrame==CurrentFrame)
        {
            return;
        }
        CurrentFrame = nextFrame;
        AnimationRenderer.PlayAnimationFrame(CurrentFrame);
        if ((mReverse&&CurrentFrame==0) || (!mReverse && CurrentFrame==mCurrentAnimationMetadata.TotalFrame-1))
        {
            if (mCallback!=null)
            {
                AnimationCallback callback = mCallback;
                int[] param = mCallbackParams;
                if (!mLoop)
                {
                    mCallback = null;
                    mCallbackParams = null;
                }
                if (callback(param))
                {
                    mCallback = null;
                    mCallbackParams = null;
                }
            }
        }
        syncAttach(CurrentFrame);
    }
    private void OnDestroy()
    {
        if (AnimationRenderer==null)
        {
            return;
        }
        AnimationRenderer.Release();
    }
}
