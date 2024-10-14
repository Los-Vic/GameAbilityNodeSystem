using GameAbilitySystem.Logic.Ability;
using NodeSystem.Editor.Windows;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GameAbilitySystem.Editor
{
    [CustomEditor(typeof(AbilityAsset))]
    public class ActiveAbilityAssetEditor:UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() != typeof(AbilityAsset)) 
                return false;
            NodeSystemEditorWindow.Open((AbilityAsset)asset);
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            base.OnInspectorGUI();
    
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph"))
            {
                NodeSystemEditorWindow.Open((AbilityAsset)target);
            }
    
            GUI.backgroundColor = oldColor;
        }
    }
}