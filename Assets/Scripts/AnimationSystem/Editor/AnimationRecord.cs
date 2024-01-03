using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Animations;
using UnityEditor;

[Serializable]
public class AnimationRecord 
{
    [Serializable]
    public class MaterialMap
    {
        public string NodeName = string.Empty;
        public Material NodeMaterial = null;
    }

    public string Name = string.Empty;
    public GameObject Model = null;
    public EAnimationType AnimationType = EAnimationType.SkinnedAnimation;
    public int Layer = 0;
    public AnimatorController Controller = null;
    public AnimationContainer[] Animations = null;
    public int AnimationFramePreSecond = -1;
    public Alias[] Attaches = new Alias[] { };
    public MaterialMap[] Materials = new MaterialMap[] { };

    [NonSerialized]
    public List<string> BoneNodeHierarchical = new List<string>();
    [NonSerialized]
    public List<string> mRenderNodes = new List<string>();

    private Dictionary<string, Alias> mAttachMaps = new Dictionary<string, Alias>();
    private Dictionary<string, MaterialMap> mMaterialMaps = new Dictionary<string, MaterialMap>();

    public AnimationRecord() { }

    public AnimationRecord(GameObject model)
    {
        Model = model;
        Name = AssetDatabase.GetAssetPath(model);
        init();
    }

    public AnimationRecord(AnimationRecord record)
    {
        Name = AssetDatabase.GetAssetPath(record.Model);
        Model = record.Model;
        AnimationType = record.AnimationType;
        Layer = record.Layer;
        Controller = record.Controller;
        Animations = record.Animations;
        AnimationFramePreSecond = record.AnimationFramePreSecond;
        Attaches = record.Attaches;
        Materials = record.Materials;
        init();
    }
    private void init()
    {
        Transform[] boneNodes = Model.GetComponentsInChildren<Transform>();
        for (int i = 0; i < boneNodes.Length; i++)
        {
            Transform boneNode = boneNodes[i];
            if (boneNode.gameObject.GetComponent<Renderer>()!=null)
            {
                continue;
            }
            if (boneNode.gameObject.GetComponent<Animator>() != null)
            {
                continue;
            }
            BoneNodeHierarchical.Add(GameObjectTools.GetHierarchical(boneNode, false));
        }
        if (Attaches!=null)
        {
            for (int i = 0; i < Attaches.Length; i++)
            {
                Alias alias = Attaches[i];
                if (!BoneNodeHierarchical.Contains(alias.Name))
                {
                    alias.Name = BoneNodeHierarchical[0];
                }
                mAttachMaps.Add(alias.AliasName, alias);
            }
        }
        syncAttaches();
        SkinnedMeshRenderer[] renderers = Model.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            string h = GameObjectTools.GetHierarchical(renderer.transform, false);
            mRenderNodes.Add(h);
        }
        if (Materials!=null)
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                MaterialMap material = Materials[i];
                if (!mRenderNodes.Contains(material.NodeName))
                {
                    continue;
                }
                mMaterialMaps.Add(material.NodeName, material);
            }
        }
        for (int i = 0; i < mRenderNodes.Count; i++)
        {
            string nodeName = mRenderNodes[i];
            if (mMaterialMaps.ContainsKey(nodeName))
            {
                continue;
            }
            mMaterialMaps.Add(nodeName, new MaterialMap() { NodeName = nodeName, NodeMaterial = null });
        }
        syncMaterials();
    }
    public void SetMaterial(string nodeName,Material mat)
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            return;
        }
        MaterialMap map;
        if (!mRenderNodes.Contains(nodeName)||!mMaterialMaps.TryGetValue(nodeName,out map))
        {
            return;
        }
        map.NodeMaterial = mat;
        syncMaterials();
    }
    public Material GetMaterial(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            return null;
        }
        MaterialMap map;
        if (!mRenderNodes.Contains(nodeName) || !mMaterialMaps.TryGetValue(nodeName, out map))
        {
            return null;
        }
        return map.NodeMaterial;
    }
    private void syncMaterials()
    {
        Materials = new MaterialMap[mMaterialMaps.Count];
        int index = 0;
        foreach (var item in mMaterialMaps)
        {
            Materials[index] = item.Value;
            index++;
        }
    }
    public void AddAttach(Alias alias)
    {
        if (mAttachMaps.ContainsKey(alias.AliasName))
        {
            mAttachMaps.Remove(alias.AliasName) ;
            return;
        }
        mAttachMaps.Add(alias.AliasName, alias);
        syncAttaches();
    }
    public void RemoveAttach(string alias)
    {
        mAttachMaps.Remove(alias);
        syncAttaches();
    }
    public bool ContainAttach(string alias)
    {
        return mAttachMaps.ContainsKey(alias);
    }
    private void syncAttaches()
    {
        Attaches = new Alias[mAttachMaps.Count];
        int index = 0;
        foreach (var item in mAttachMaps)
        {
            Attaches[index] = item.Value;
            index++;
        }
    }
}
