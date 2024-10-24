using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem
{
    public class NodeSystemGraphRunner:MonoBehaviour
    {
        public NodeSystemGraphAsset asset;

        private readonly Dictionary<string, NodeSystemNode> _nodeIdMap = new();
        private readonly Dictionary<string, NodeSystemPort> _portIdMap = new();
        private readonly Dictionary<string, List<string>> _nodePortsMap = new();
        private readonly Dictionary<string, NodeSystemNodeRunner> _nodeRunners = new();
        
        //To execute flow node, we need output value of dependent value nodes
        private readonly Dictionary<string, List<string>> _nodeValDependencyMap = new();
        
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();

        private readonly List<string> _toRunValueNodeList = new();
        
        private void Awake()
        {
            InitRunner(asset);
        }

        private void Start()
        {
            NodeSystemNode startNode = null;
            foreach (var node in asset.nodes)
            {
                var attribute = node.GetType().GetCustomAttribute<NodeAttribute>();
                if (attribute is not { NodeCategory: ENodeCategory.Start }) 
                    continue;
                startNode = node;
                break;
            }
            if(startNode == null)
                return;
        }

        private void InitRunner(NodeSystemGraphAsset graphAsset)
        {
            foreach (var node in graphAsset.nodes)
            {
                _nodeIdMap.Add(node.Id, node);
                _nodePortsMap.Add(node.Id, new List<string>());
            }

            foreach (var port in graphAsset.ports)
            {
                _portIdMap.Add(port.Id, port);
                if (_nodePortsMap.TryGetValue(port.belongNodeId, out var portList))
                {
                    portList.Add(port.Id);
                }
            }
        }
        
    }
}