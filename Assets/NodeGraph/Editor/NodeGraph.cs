using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Gray.NG.Editor
{
    [Serializable]
    [Graph(ASSET_EXTENSION)]
    public class NodeGraph : Graph
    {
        public const string DEFAULT_NAME = "Node Graph";
        public const string ASSET_EXTENSION = "ng";
        
        [MenuItem("Assets/Create/Node Graph")]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<NodeGraph>(DEFAULT_NAME);
        }

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);
            CheckGraphErrors(graphLogger);
        }
        
        private void CheckGraphErrors(GraphLogger graphLogger)
        {
            graphLogger.Log("Checking for graph errors...");
        }
    }
}