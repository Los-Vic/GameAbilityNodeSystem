using NS;
using NSEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NodeSystem.NodeSystemDemo.Editor
{
    [CustomEditor(typeof(DemoGraphAsset))]
    public class NodeDemoAssetEditor:UnityEditor.Editor
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
                NodeEditorWindow.Open<NodeDemoEditorWindow>((NodeGraphAsset)target);
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