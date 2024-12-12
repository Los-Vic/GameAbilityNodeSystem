using System.Collections.Generic;
using NS;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NSEditor
{
    public class NodeEditorWindow :EditorWindow
    {
        public static void Open<T>(NodeGraphAsset target) where T : NodeEditorWindow
        {
            var windows = Resources.FindObjectsOfTypeAll<T>();
            foreach (var w in windows)
            {
                if (w.currentGraphAsset == null)
                {
                    w.Close();
                    continue;
                }
                
                if (w.currentGraphAsset == target)
                {
                    w.Focus();
                    w.Reload();
                    return;
                }
            }
            
            var window = CreateWindow<T>(typeof(T), typeof(SceneView));
            window.titleContent = new GUIContent($"{target.name}",
                EditorGUIUtility.ObjectContent(target, typeof(T)).image);
            window.Load(target);
        }
        
        [SerializeField]
        private NodeGraphAsset currentGraphAsset;
        private NodeGraphView _currentView;
        private SerializedObject _serializedObject;
        
        
        private void OnEnable()
        {
            if (currentGraphAsset != null)
                DrawGraph();
        }
        
        private void Load(NodeGraphAsset target)
        {
            currentGraphAsset = target;
            DrawGraph();
        }
        private void Reload()
        {
            _currentView?.ReDrawGraph();
        }

        private void DrawGraph()
        {
            _serializedObject = new SerializedObject(currentGraphAsset);
            _currentView = CreateGraphView();
            _currentView.StretchToParentSize();
            rootVisualElement.Add(_currentView);
        }
        
        #region Virtual Functions
        
        protected virtual NodeGraphView CreateGraphView()
        {
            return new NodeGraphView(_serializedObject, this);
        }

        public virtual NodeSearchProvider CreateSearchProvider()
        {
            var provider = CreateInstance<NodeSearchProvider>();
            provider.SetScopeList(new List<int>(0));

            return provider;
        }

        public virtual EditorNode CreateEditorNode()
        {
            return new NodeSystemEditorNode();
        }
        
        #endregion
      
    }
}