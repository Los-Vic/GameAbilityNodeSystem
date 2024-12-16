﻿using GAS.Logic;
using NS;
using NSEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AbilityAsset))]
    public class AbilityAssetEditor:OdinEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            //Open Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph", GUILayout.Height(30)))
            {
                NodeEditorWindow.Open<AbilityAssetEditorWindow>((NodeGraphAsset)target);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("ValidateGraph", GUILayout.Height(30)))
            {
                NodeGraphAssetEditorUtility.ValidateGraph(serializedObject);
            }
            GUI.backgroundColor = oldColor;
        }
        
    }
}