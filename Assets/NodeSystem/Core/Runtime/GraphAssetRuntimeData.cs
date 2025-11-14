using System.Collections.Generic;
using System.Reflection;
using System;
using Gameplay.Common;

namespace NS
{
    public class GraphAssetRuntimeData
    {
        public NodeGraphAsset Asset { get; private set; }
        
        //Query
        private readonly Dictionary<string, Node> _nodeIdMap = new();
        private readonly Dictionary<string, NodePort> _portIdMap = new();
        private readonly Dictionary<string, List<string>> _nodePortsMap = new();
        //To execute action node, we need output value of dependent value && reroute nodes
        private readonly Dictionary<string, List<string>> _nodeValDependencyMap = new();
        
        private readonly Dictionary<(Type, int), string> _entryNodeMap = new();
        private readonly Dictionary<Type, List<(int, string)>> _entryTypePairListMap = new();
        
        //Transient
        private readonly List<string> _toRunNodeList = new();

        public void Init(NodeGraphAsset asset)
        {
            Asset = asset;
            
            //Construct NodeIdMap & NodePortsMap
            foreach (var node in Asset.nodes)
            {
                _nodeIdMap.Add(node.Id, node);
                _nodePortsMap.Add(node.Id, new List<string>());

                if (!node.IsEntryNode()) 
                    continue;

                var hasPortalEnum = false;
                var nodeType = node.GetType();
                foreach (var fieldInfo in nodeType.GetFields())
                {
                    if (fieldInfo.GetCustomAttribute<EntryAttribute>() == null)
                        continue;
                    
                    hasPortalEnum = true;
                    var enumVal = (int)fieldInfo.GetValue(node);
                    if (!_entryNodeMap.TryAdd((nodeType, enumVal), node.Id))
                    {
                        GameLogger.LogError($"Fail to add portal node to ports map. Node type: {nodeType}, portal val: {enumVal}");
                    }
                    else
                    {
                        _entryTypePairListMap.TryAdd(nodeType, new List<(int, string)>());
                        _entryTypePairListMap[nodeType].Add((enumVal, node.Id));
                    }
                }

                if (hasPortalEnum) 
                    continue;
                
                if (!_entryNodeMap.TryAdd((nodeType, 0), node.Id))
                {
                    GameLogger.LogError($"Fail to add portal node to ports map. Node type: {nodeType}");
                }
                else
                {
                    _entryTypePairListMap.TryAdd(nodeType, new List<(int, string)>());
                    _entryTypePairListMap[nodeType].Add((0, node.Id));
                }

            }

            //Construct PortIdMap & NodePortsMap
            foreach (var port in Asset.ports)
            {
                _portIdMap.Add(port.Id, port);
                if (_nodePortsMap.TryGetValue(port.belongNodeId, out var portList))
                {
                    portList.Add(port.Id);
                }
            }

            //Construct NodeValDependencyMap
            foreach (var node in Asset.nodes)
            {
                if (!node.IsActionNode())
                    continue;

                var valueNodeList = new List<string>();
                _nodeValDependencyMap.Add(node.Id, valueNodeList);
                
                _toRunNodeList.Clear();
                _toRunNodeList.Add(node.Id);

                while (_toRunNodeList.Count > 0)
                {
                    foreach (var portId in _nodePortsMap[_toRunNodeList[0]])
                    {
                        var port = _portIdMap[portId];
                        if(port.IsFlowPort() || port.direction == EPortDirection.Output)
                            continue;
                    
                        if(!NodePort.IsValidPortId(port.connectPortId))
                            continue;
                    
                        var connectPort = _portIdMap[port.connectPortId];
                        var connectNode = _nodeIdMap[connectPort.belongNodeId];
                        if (!connectNode.IsValueNode()) 
                            continue;
                        valueNodeList.Add(connectNode.Id);
                        _toRunNodeList.Add(connectNode.Id);
                    }
                    
                    _toRunNodeList.RemoveAt(0);
                }
            }
        }

        public Node GetNodeById(string id) => _nodeIdMap.GetValueOrDefault(id);
        public NodePort GetPortById(string id) => _portIdMap.GetValueOrDefault(id);
        public List<string> GetPortIdsOfNode(string nodeId) => _nodePortsMap.GetValueOrDefault(nodeId, new List<string>());
        public List<string> GetDependentNodeIds(string nodeId) => _nodeValDependencyMap.GetValueOrDefault(nodeId, new List<string>());
        public string GetEntryNodeId(Type nodeType, int portalVal = 0) => _entryNodeMap.GetValueOrDefault((nodeType, portalVal));
        public List<(int, string)> GetEntryNodePairList(Type nodeType) => _entryTypePairListMap.GetValueOrDefault(nodeType);

    }
}