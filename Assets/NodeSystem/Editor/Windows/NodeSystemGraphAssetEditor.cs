using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NodeSystem.Editor.Windows
{
    [CustomEditor(typeof(NodeSystemGraphAsset))]
    public class NodeSystemGraphAssetEditor:UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() != typeof(NodeSystemGraphAsset)) 
                return false;
            NodeSystemEditorWindow.Open((NodeSystemGraphAsset)asset);
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //ReadOnly
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
            
            //Open Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph"))
            {
                NodeSystemEditorWindow.Open((NodeSystemGraphAsset)target);
            }
            GUI.backgroundColor = oldColor;
        }
    }
}