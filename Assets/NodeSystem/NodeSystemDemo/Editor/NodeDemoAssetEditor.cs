using NS;
using NSEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NodeSystem.NodeSystemDemo.Editor
{
    [CustomEditor(typeof(DemoGraphAsset), true)]
    public class NodeDemoAssetEditor:UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            var type = asset.GetType();
            if (!typeof(NodeGraphAsset).IsAssignableFrom(type)) 
                return false;
            NodeEditorWindow.Open<NodeDemoEditorWindow>((NodeGraphAsset)asset);
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
                NodeEditorWindow.Open<NodeDemoEditorWindow>((NodeGraphAsset)target);
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