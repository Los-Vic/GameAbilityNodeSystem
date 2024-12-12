using GAS.Logic;
using NS;
using NSEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AbilityAsset), true)]
    public class AbilityAssetEditor:OdinEditor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() != typeof(AbilityAsset)) 
                return false;
            NodeEditorWindow.Open<AbilityAssetEditorWindow>((NodeGraphAsset)asset);
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            //Open Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph"))
            {
                NodeEditorWindow.Open<AbilityAssetEditorWindow>((NodeGraphAsset)target);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("ValidateGraph"))
            {
                NodeGraphAssetEditorUtility.ValidateGraph(serializedObject);
            }
            GUI.backgroundColor = oldColor;
        }
        
    }
}