using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System;

public class AnimationBuilderWindow : EditorWindow
{
    private AnimationRecords mRecords;
    private Vector2 mScrollPosition = Vector2.zero;
    private GameObject mModel;
    private AnimationRecord mCurrentRecord = new AnimationRecord();
    [MenuItem("Tools/AnimationBuilder")]
    public static void OpenWindow()
    {
        AnimationBuilderWindow window = GetWindow<AnimationBuilderWindow>(false, "AnimationBuilder",true);
        window.position = new Rect(50, 150, 500, 500);
        window.minSize = new Vector2(400,400);
        window.titleContent = new GUIContent("AnimationBuilder");
        window.initConfig();
        window.Show();
    }
    private void initConfig()
    {
        mRecords = AnimationBuilderUtil.GetAnimationRecords();
    }
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, position.width - 20, position.height - 20));
        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, false, true);
        if (EditorApplication.isPlaying)
        {
            Close();
        }
        if (EditorApplication.isCompiling)
        {
            Close();
        }
        GameObject newModel = EditorGUILayout.ObjectField("模型", mModel, typeof(GameObject), false) as GameObject;
        if (newModel!=mModel)
        {
            if (newModel!=null&&(PrefabUtility.GetPrefabAssetType(newModel)!= PrefabAssetType.Model||newModel.GetComponentsInChildren<SkinnedMeshRenderer>().Length<=0))
            {
                EditorUtility.DisplayDialog("请选择模型文件", "请选择模型文件","OK");
            }
            else
            {
                mModel = newModel;
                init();
            }
        }
        EditorGUI.BeginDisabledGroup(mModel == null);
        mCurrentRecord.AnimationType = (EAnimationType)EditorGUILayout.EnumPopup("动画类型", mCurrentRecord.AnimationType);
        mCurrentRecord.Layer = EditorGUILayout.LayerField("模型所在层", mCurrentRecord.Layer);
        mCurrentRecord.Controller = EditorGUILayout.ObjectField("动画控制器", mCurrentRecord.Controller, typeof(AnimatorController), false) as AnimatorController;
        drawAttach();
        drawAnimationFramePreSecond();
        drawAnimations();
        drawMaterials();
        drawButton();
        EditorGUI.EndDisabledGroup();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private void init()
    {
        if (mModel==null)
        {
            mCurrentRecord = new AnimationRecord();
        }
        else
        {
            AnimationRecord record = mRecords.GetRecord(mModel);
            if (record==null)
            {
                mCurrentRecord = new AnimationRecord(mModel);
            }
            else
            {
                mCurrentRecord = new AnimationRecord(record);
            }
        }
    }
    private void drawAttach()
    {
        GUITools.BeginContents(GUITools.Styles.HelpBoxStyle);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("挂载点");
        if (GUILayout.Button("+"))
        {
            if (mCurrentRecord.ContainAttach("None"))
            {
                EditorUtility.DisplayDialog("请优先设置已有字段", "请优先设置已有字段","OK");
            }
            else
            {
                mCurrentRecord.AddAttach(new Alias { AliasName="None",Name=mCurrentRecord.BoneNodeHierarchical[0]});
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        List<string> removeKeys = new List<string>();
        Dictionary<string, Alias> newAttach = new Dictionary<string, Alias>();
        foreach (var item in mCurrentRecord.Attaches)
        {
            EditorGUILayout.BeginHorizontal();
            string aliasName = EditorGUILayout.TextField(item.AliasName);
            int curIndex = mCurrentRecord.BoneNodeHierarchical.IndexOf(item.Name);
            int index = EditorGUILayout.Popup(curIndex, mCurrentRecord.BoneNodeHierarchical.ToArray());
            if (index!=curIndex||aliasName!=item.AliasName)
            {
                if (aliasName!=item.AliasName)
                {
                    if (mCurrentRecord.ContainAttach(aliasName))
                    {
                        EditorUtility.DisplayDialog("已包含别名", "已包含别名" + aliasName + "的挂载点，请重新设置", "OK");
                        continue;
                    }
                }
                removeKeys.Add(item.AliasName);
                newAttach.Add(aliasName, new Alias() { AliasName = aliasName, Name = mCurrentRecord.BoneNodeHierarchical[index] });
            }
            if (GUILayout.Button("-"))
            {
                removeKeys.Add(aliasName);
            }
            EditorGUILayout.EndHorizontal();
        }
        for (int i = 0; i < removeKeys.Count; i++)
        {
            mCurrentRecord.RemoveAttach(removeKeys[i]);
        }
        foreach (var item in newAttach)
        {
            mCurrentRecord.AddAttach(item.Value);
        }
        EditorGUILayout.EndVertical();
        GUITools.EndContents();
    }
    private void drawAnimationFramePreSecond()
    {
        GUITools.BeginContents(GUITools.Styles.HelpBoxStyle);
        EditorGUILayout.BeginVertical();
        mCurrentRecord.AnimationFramePreSecond = EditorGUILayout.IntField("每秒帧数:", mCurrentRecord.AnimationFramePreSecond);
        EditorGUILayout.HelpBox("默认-1为按照动画设置决定每秒帧数", MessageType.Info);
        EditorGUILayout.EndVertical();
        GUITools.EndContents();
    }
    private void drawAnimations()
    {
        GUITools.BeginContents(GUITools.Styles.HelpBoxStyle);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("动画片段");
        if (GUILayout.Button("自动识别"))
        {
            List<AnimationContainer> searchedAnimations = new List<AnimationContainer>();
            if (mCurrentRecord.Controller==null)
            {
                string dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mCurrentRecord.Model));
                List<string> modelFiles = new List<string>();
                modelFiles.AddRange(Directory.GetFiles(dirPath, "*@*.fbx", SearchOption.TopDirectoryOnly));
                for (int i = 0; i < modelFiles.Count; i++)
                {
                    AnimationClip animation = AssetDatabase.LoadAssetAtPath<AnimationClip>(modelFiles[i]);
                    if (animation==null)
                    {
                        continue;
                    }
                    searchedAnimations.Add(new AnimationContainer() { AnimationName = animation.name, Animation = animation });
                }
            }
            else
            {
                ChildAnimatorState[] childAnimatorStates = mCurrentRecord.Controller.layers[0].stateMachine.states;
                for (int i = 0; i < childAnimatorStates.Length; i++)
                {
                    ChildAnimatorState state = childAnimatorStates[i];
                    string animationName = state.state.name;
                    AnimationClip animation = state.state.motion as AnimationClip;
                    if (animation == null)
                    {
                        continue;
                    }
                    searchedAnimations.Add(new AnimationContainer() { AnimationName = animation.name, Animation = animation });
                }
            }
            if (mCurrentRecord.Animations==null)
            {
                mCurrentRecord.Animations = searchedAnimations.ToArray();
            }
            else
            {
                List<AnimationContainer> animations = new List<AnimationContainer>();
                for (int i = 0; i < mCurrentRecord.Animations.Length; i++)
                {
                    AnimationContainer alias = mCurrentRecord.Animations[i];
                    if (string.IsNullOrEmpty(alias.AnimationName) || alias.Animation == null)
                    {
                        continue;
                    }
                    animations.Add(alias);
                }
                for (int i = 0; i < searchedAnimations.Count; i++)
                {
                    AnimationContainer alias = searchedAnimations[i];
                    bool contain = false;
                    for (int j = 0; j < animations.Count; j++)
                    {
                        AnimationContainer alias2 = animations[j];
                        if (alias.AnimationName==alias2.AnimationName)
                        {
                            contain = true;
                            break;
                        }
                    }
                    if (contain)
                    {
                        continue;
                    }
                    animations.Add(alias);
                }
                mCurrentRecord.Animations = animations.ToArray();
            }
        }
        if (GUILayout.Button("+"))
        {
            if (mCurrentRecord.Animations==null)
            {
                mCurrentRecord.Animations = new AnimationContainer[1] { new AnimationContainer()};
            }
            else
            {
                if (mCurrentRecord.Animations[mCurrentRecord.Animations.Length-1].Animation==null)
                {
                    EditorUtility.DisplayDialog("请优先设置已有字段", "请优先设置已有字段", "OK");
                }
                else
                {
                    AnimationContainer[] newAlias = new AnimationContainer[mCurrentRecord.Animations.Length + 1];
                    Array.Copy(mCurrentRecord.Animations, newAlias, mCurrentRecord.Animations.Length);
                    newAlias[newAlias.Length - 1] = new AnimationContainer();
                    mCurrentRecord.Animations = newAlias;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        if (mCurrentRecord.Animations!=null)
        {
            List<int> removeIndex = new List<int>();
            for (int i = 0; i < mCurrentRecord.Animations.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                AnimationContainer alias = mCurrentRecord.Animations[i];
                alias.AnimationName = EditorGUILayout.TextField(alias.Animation != null ? alias.AnimationName : string.Empty);
                alias.Animation = EditorGUILayout.ObjectField(alias.Animation, typeof(AnimationClip), false) as AnimationClip;
                if (string.IsNullOrEmpty(alias.AnimationName)&&alias.Animation!=null)
                {
                    alias.AnimationName = alias.Animation.name;
                }
                alias.AnimationName = verifyAnimationName(alias, alias.AnimationName, 0);
                if (GUILayout.Button("-"))
                {
                    removeIndex.Add(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            List<AnimationContainer> animations = new List<AnimationContainer>();
            for (int i = 0; i < mCurrentRecord.Animations.Length; i++)
            {
                bool remove = false;
                for (int j = 0; j < removeIndex.Count; j++)
                {
                    if (i==removeIndex[j])
                    {
                        remove = true;
                        break;
                    }
                }
                if (remove)
                {
                    continue;
                }
                animations.Add(mCurrentRecord.Animations[i]);
            }
            mCurrentRecord.Animations = animations.Count <= 0 ? null : animations.ToArray();
        }
        EditorGUILayout.EndVertical();
        GUITools.EndContents();
    }
    private string verifyAnimationName(AnimationContainer alias,string name,int index)
    {
        if (!string.IsNullOrEmpty(alias.AnimationName))
        {
            for (int i = 0; i < mCurrentRecord.Animations.Length; i++)
            {
                if (mCurrentRecord.Animations[i]==alias)
                {
                    continue;
                }
                if (mCurrentRecord.Animations[i].AnimationName==name)
                {
                    name = alias.AnimationName + " (" + index + ")";
                    name = verifyAnimationName(alias,name, ++index);
                }
            }
        }
        return name;
    }
    private void drawMaterials()
    {
        GUITools.BeginContents(GUITools.Styles.HelpBoxStyle);
        EditorGUILayout.LabelField("材质球");
        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical();
        foreach (var item in mCurrentRecord.Materials)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(item.NodeName);
            Material mat = EditorGUILayout.ObjectField(item.NodeMaterial, typeof(Material), false) as Material;
            if (mat!=item.NodeMaterial)
            {
                mCurrentRecord.SetMaterial(item.NodeName, mat);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        GUITools.EndContents();
    }
    private void drawButton()
    {
        GUITools.BeginContents(GUITools.Styles.HelpBoxStyle);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build"))
        {
            if (mCurrentRecord.Model==null)
            {
                EditorUtility.DisplayDialog("请选择模型", "请选择模型", "OK");
            }
            else if (mCurrentRecord.Animations == null || (mCurrentRecord.Animations.Length == 1 && mCurrentRecord.Animations[0].Animation == null))
            {
                EditorUtility.DisplayDialog("请选择动画", "请选择动画", "OK");
            }
            else if (!verify())
            {
                EditorUtility.DisplayDialog("错误", "请根据提示解决错误后再执行Build", "OK");
            }
            else
            {
                AnimationBuilderBase builder = new GPUSkinnedAnimationBuilder(mCurrentRecord);
                builder.Bake();
                mRecords.AddRecord(mCurrentRecord.Model, mCurrentRecord);
                AnimationRecords records = ScriptableObject.CreateInstance<AnimationRecords>();
                records.Records = mRecords.Records;
                AssetDatabase.CreateAsset(records, AnimationBuilderUtil.AnimationRecordsPath);
                AssetDatabase.Refresh();
                init();
            }
        }
        if (GUILayout.Button("还原"))
        {
            init();
        }
        EditorGUILayout.EndVertical();
        GUITools.EndContents();
    }
    private bool verify()
    {
        List<string> alias = new List<string>();
        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
        for (int i = 0; i < mCurrentRecord.Animations.Length; i++)
        {
            AnimationContainer ac = mCurrentRecord.Animations[i];
            if (alias.Contains(ac.AnimationName))
            {
                return false;
            }
            alias.Add(ac.AnimationName);
            if (ac.Animation==null)
            {
                return false;
            }
            if (clips.ContainsKey(ac.Animation.name))
            {
                if (clips[ac.Animation.name]!=ac.Animation)
                {
                    return false;
                }
            }
            else
            {
                clips.Add(ac.Animation.name, ac.Animation);
            }
        }
        return true;
    }
}
