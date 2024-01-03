using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;

public class AnimationBuilderBase 
{
    public const string PlayerPrefabPathFormat = "{0}/{1}.prefab";
    public const string AnimationAttachPathFormat = "{0}/{1}_Attach.asset";
    public const string AnimationBoundsPathForamt = "{0}/{1}_Bounds.asset";
    public const string TempControllerPathFormat = "{0}/AnimationBuilder.controller";
    public GameObject ModelObj;
    public Animator AnimatorObj;
    public string ModelDirectory;
    public string SaveDirectory;
    public SkinnedMeshRenderer[] SkinnedRenderers;
    public string[] BoneHierarchicals;
    public AttachManifest AttachManifest;
    protected int[] AnimationClipIndices;
    protected AnimationRendererBase AnimationRenderer;
    public AnimationBoundsManifest AnimationBoundsManifest;
    public AnimationRecord Record;
    public AnimationBuilderBase(AnimationRecord record)
    {
        Record = record;
        ModelObj = GameObjectTools.Instantiate(Record.Model,null,Record.Model.name,Vector3.zero,Quaternion.identity,Vector3.one);
        ModelDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Record.Model)).Replace("\\","/");
        SaveDirectory=ModelDirectory.Replace("/Models/","/Prefabs/");
        FileTools.VerifyPath(SaveDirectory);
        AnimatorObj = ModelObj.GetComponent<Animator>();
        AnimatorObj.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        SkinnedRenderers = ModelObj.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < SkinnedRenderers.Length; i++)
        {
            SkinnedRenderers[i].updateWhenOffscreen = true;
        }
        BoneHierarchicals = AnimationBuilderUtil.GetAllBone(Record.Model);
        AnimationClipIndices = AnimationBuilderUtil.GetAnimationClipCount(Record.Animations);
        AnimationBoundsManifest = new AnimationBoundsManifest();
        AnimationBoundsManifest.AnimationBoundsMetadatas = new AnimationBoundsMetadata[Record.Animations.Length];
        if (Record.Attaches.Length>0)
        {
            List<AttachMetadata> metadatas = new List<AttachMetadata>();
            for (int i = 0; i < Record.Attaches.Length; i++)
            {
                Alias item = Record.Attaches[i];
                string attachPath = item.Name;
                string attachAlias = item.AliasName;
                AttachMetadata metadata = new AttachMetadata();
                metadata.AttachAlias = new Alias() { AliasName=attachAlias,Name=attachPath};
                Dictionary<string, AttachAnimationMetadata> attachMetadataDict = new Dictionary<string, AttachAnimationMetadata>();
                for (int j = 0; j < AnimationClipIndices.Length; j++)
                {
                    AnimationContainer animationAlias = Record.Animations[AnimationClipIndices[j]];
                    if (attachMetadataDict.ContainsKey(animationAlias.Animation.name))
                    {
                        continue;
                    }
                    AttachAnimationMetadata animationMetadata = new AttachAnimationMetadata();
                    animationMetadata.AnimationName = animationAlias.Animation.name;
                    int length = Mathf.FloorToInt(animationAlias.Animation.length * (Record.AnimationFramePreSecond == -1 ? animationAlias.Animation.frameRate : Record.AnimationFramePreSecond));
                    animationMetadata.Positions = new Vector3[length];
                    animationMetadata.Rotations = new Quaternion[length];
                    attachMetadataDict.Add(animationAlias.Animation.name, animationMetadata);
                }
                metadata.AnimationMetadatas = new AttachAnimationMetadata[attachMetadataDict.Count];
                int index = 0;
                foreach (var animationMetadata in attachMetadataDict)
                {
                    metadata.AnimationMetadatas[index++] = animationMetadata.Value;
                }
                metadata.Init();
                metadatas.Add(metadata);
            }
            if (metadatas.Count>0)
            {
                AttachManifest = new AttachManifest();
                AttachManifest.AttachMetadatas = metadatas.ToArray();
                AttachManifest.Init();
            }
        }
    }
    protected void SwitchAnimation(AnimationClip animation)
    {
        if (animation==null)
        {
            return;
        }
        if (AnimatorObj.runtimeAnimatorController!=null)
        {
            AssetDatabase.DeleteAsset(string.Format(TempControllerPathFormat,SaveDirectory));
        }
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPathWithClip(string.Format(TempControllerPathFormat, SaveDirectory), animation);
        AssetDatabase.Refresh();
        AnimatorObj.runtimeAnimatorController = controller;
        AnimatorObj.Rebind();
    }
    protected virtual void Update(int animationIndex)
    {

    }
    public void Bake()
    {
        for (int i = 0; i < AnimationClipIndices.Length; i++)
        {
            int index = AnimationClipIndices[i];
            AnimationContainer alias = Record.Animations[index];
            AnimationClip animation = alias.Animation;
            if (animation==null)
            {
                continue;
            }
            AnimationBoundsMetadata boundsMetadata = new AnimationBoundsMetadata();
            AnimationBoundsManifest.AnimationBoundsMetadatas[index] = boundsMetadata;
            boundsMetadata.AnimationName = alias.Animation.name;
            for (int j = 0; j < SkinnedRenderers.Length; j++)
            {
                boundsMetadata.ModelBounds.Encapsulate(SkinnedRenderers[j].bounds);
            }
            float rate = Record.AnimationFramePreSecond == -1 ? animation.frameRate : Record.AnimationFramePreSecond;
            float step = 1f / rate;
            int length = Mathf.FloorToInt(animation.length * rate);
            SwitchAnimation(animation);
            AnimatorObj.StopPlayback();
            AnimatorObj.StartRecording(length);
            for (int aIndex = 0; aIndex < length; aIndex++)
            {
                AnimatorObj.Update(aIndex<=0?0f:step);
            }
            AnimatorObj.StopRecording();
            AnimatorObj.StartPlayback();
            float timer = 0f;
            for (int aIndex = 0; aIndex < length; aIndex++,timer+=step)
            {
                AnimatorObj.playbackTime = timer;
                AnimatorObj.Update(0f);
                Update(i);
                if (AttachManifest==null)
                {
                    continue;
                }
                foreach (var metadata in AttachManifest.AttachMetadataDict)
                {
                    AttachAnimationMetadata animationMetadata;
                    if (!metadata.Value.AnimationMetadataDict.TryGetValue(alias.Animation.name,out animationMetadata))
                    {
                        continue;
                    }
                    Transform attachNode = ModelObj.transform.Find(metadata.Value.AttachAlias.Name);
                    animationMetadata.Positions[aIndex] = attachNode.localToWorldMatrix.GetPosition();
                    animationMetadata.Rotations[aIndex] = attachNode.localToWorldMatrix.GetRotation();
                }
            }
        }
        save();
        release();
    }
    private void save()
    {
        onSave();
        if (AttachManifest!=null)
        {
            string attachAssetPath = string.Format(AnimationAttachPathFormat, SaveDirectory, Record.Model.name);
            AssetDatabase.CreateAsset(AttachManifest, attachAssetPath);
        }
        string boundsAssetPath = string.Format(AnimationBoundsPathForamt, SaveDirectory, Record.Model.name);
        AssetDatabase.CreateAsset(AnimationBoundsManifest, boundsAssetPath);
        GameObject prefab = GameObjectTools.Instantiate(null, null, Record.Model.name, Vector3.zero, Quaternion.identity, Vector3.one);
        prefab.layer = Record.Layer;
        AnimationPlayer player = prefab.AddComponent<AnimationPlayer>();
        player.Identity = string.Format(PlayerPrefabPathFormat, SaveDirectory.Replace("Assets/", string.Empty), Record.Model.name);
        player.AnimationType = Record.AnimationType;
        player.AttachManifest = AttachManifest;
        player.AnimationBoundsManifest = AnimationBoundsManifest;
        List<Alias> aliases = new List<Alias>();
        for (int i = 0; i < Record.Animations.Length; i++)
        {
            Alias alias = new Alias();
            alias.AliasName = Record.Animations[i].AnimationName;
            alias.Name = Record.Animations[i].Animation.name;
            aliases.Add(alias);
        }
        player.AnimationAlias = aliases.ToArray();
        AnimationRenderer.gameObject.name = "AnimationRenderer";
        AnimationRenderer.transform.parent = prefab.transform;
        AnimationRenderer.transform.localPosition = Vector3.zero;
        AnimationRenderer.transform.localRotation = Quaternion.identity;
        AnimationRenderer.transform.localScale = Vector3.one;
        player.AnimationRenderer = AnimationRenderer;
        string playerPath = string.Format(PlayerPrefabPathFormat, SaveDirectory, Record.Model.name);
        bool success;
        PrefabUtility.SaveAsPrefabAsset(prefab, playerPath, out success);
        Debug.Log("Success:"+ playerPath,AssetDatabase.LoadAssetAtPath<Object>(playerPath));
        Object.DestroyImmediate(prefab);
        AssetDatabase.SaveAssets();
    }
    private void release()
    {
        onRelease();
        if (AnimatorObj.runtimeAnimatorController != null)
        {
            AssetDatabase.DeleteAsset(string.Format(TempControllerPathFormat, SaveDirectory));
        }
        Object.DestroyImmediate(ModelObj);
    }
    protected virtual void onSave()

    { }
    protected virtual void onRelease()
    { }
}
