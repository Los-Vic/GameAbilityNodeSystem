using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Gray.NG.Editor
{
    
    [ScriptedImporter(1, NodeGraph.ASSET_EXTENSION)]
    public class NodeGraphImporter:ScriptedImporter
    {
        private readonly Dictionary<GraphNode, RuntimeNode> _graphNodeRuntimeNodeMap = new();
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<NodeGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"Failed to load Visual node graph asset: {ctx.assetPath}");
                return;
            }

            var runtimeAsset = ScriptableObject.CreateInstance<RuntimeNodeGraph>();
            var graphNodes = graph.GetNodes().OfType<GraphNode>();
            _graphNodeRuntimeNodeMap.Clear();
            
            //first loop to create runtime node
            foreach (var graphNode in graphNodes)
            {
                var runtimeNode = graphNode.CreateRuntimeNode();
                if (runtimeNode == null)
                {
                    Debug.LogError($"failed to create runtime node of {graphNode.GetType()}");
                    continue;
                }

                if (graphNode.IsTickable())
                {
                    runtimeAsset.isTickable = true;
                }
                runtimeNode.guid = graphNode.guid;
                _graphNodeRuntimeNodeMap.Add(graphNode, runtimeNode);
                runtimeAsset.entryNodeIndexed.Add(runtimeAsset.nodes.Count);
                runtimeAsset.nodes.Add(runtimeNode);
            }
            
            //second loop to assign port val
            foreach(var (node, runtimeNode) in _graphNodeRuntimeNodeMap)
            {
                node.AssignRuntimeNodePortValues(runtimeNode, this);
            }
            
            // Add the runtime object to the graph asset and set it to be the main asset.
            // This allows the same asset to be used in inspectors wherever a runtime asset is expected.
            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
            
            _graphNodeRuntimeNodeMap.Clear();
        }

        public RuntimeNode GetRuntimeNodeOfGraphNode(GraphNode graphNode)
        {
            return _graphNodeRuntimeNodeMap.GetValueOrDefault(graphNode);
        }
    }
}