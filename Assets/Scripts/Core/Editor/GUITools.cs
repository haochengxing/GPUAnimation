using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUITools
{
    public static class Styles
    {
        private static GUIStyle mHelpBoxStyle;
        public static GUIStyle HelpBoxStyle
        {
            get
            {
                if (mHelpBoxStyle == null)
                {
                    mHelpBoxStyle = new GUIStyle("HelpBox");
                    mHelpBoxStyle.padding = new RectOffset(4,4,4,4);
                    mHelpBoxStyle.margin = new RectOffset(0, 0, 4, 4);
                }
                return mHelpBoxStyle;
            }
        }
    }

    public static void BeginContents(GUIStyle style)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.BeginHorizontal(style,GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndContents()
    {
        try
        {
            GUILayout.Space(3f);
        }
        catch (Exception)
        { 
        }

        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(3f);
        GUILayout.EndHorizontal();

        GUILayout.Space(3f);
    }
}
