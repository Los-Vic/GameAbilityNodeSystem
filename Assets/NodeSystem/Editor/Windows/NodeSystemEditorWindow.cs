using UnityEditor;
using UnityEngine;

namespace NodeSystem.Editor.Windows
{
    public class NodeSystemEditorWindow : EditorWindow
    {
        public static void Open(NodeSystemGraphAsset target)
        {
            var windows = Resources.FindObjectsOfTypeAll<NodeSystemEditorWindow>();
            foreach (var w in windows)
            {
                if (w._currentGraphAsset == null)
                {
                    w.Close();
                    continue;
                }
                
                if (w._currentGraphAsset == target)
                {
                    w.Focus();
                    return;
                }
            }

            var window = CreateWindow<NodeSystemEditorWindow>(typeof(NodeSystemEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent($"{target.name}",
                EditorGUIUtility.ObjectContent(target, typeof(NodeSystemGraphAsset)).image);
            window.Load(target);
        }

        private NodeSystemGraphAsset _currentGraphAsset;
        private NodeSystemGraphView _currentView;
        private SerializedObject _serializedObject;

        private void OnEnable()
        {
            if (_currentGraphAsset != null)
                DrawGraph();
        }

        private void Load(NodeSystemGraphAsset target)
        {
            _currentGraphAsset = target;
            DrawGraph();
        }

        private void DrawGraph()
        {
            _serializedObject = new SerializedObject(_currentGraphAsset);
            _currentView = new NodeSystemGraphView(_serializedObject, this);
            rootVisualElement.Add(_currentView);
        }
    }
}