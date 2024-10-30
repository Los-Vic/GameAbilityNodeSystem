using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    public class GraphAssetRuntimeData
    {
        public NodeSystemGraphAsset Asset;
        public readonly Dictionary<string, NodeSystemNode> NodeIdMap = new();
        public readonly Dictionary<string, NodeSystemPort> PortIdMap = new();
        public readonly Dictionary<string, List<string>> NodePortsMap = new();
        //To execute flow node, we need output value of dependent value nodes
        public readonly Dictionary<string, List<string>> NodeValDependencyMap = new();
        public string StartNodeId;
        
        private readonly List<string> _toRunNodeList = new();

        public void Init(NodeSystemGraphAsset asset)
        {
            Asset = asset;
            
            //Construct NodeIdMap & NodePortsMap
            foreach (var node in Asset.nodes)
            {
                NodeIdMap.Add(node.Id, node);
                NodePortsMap.Add(node.Id, new List<string>());
            }

            //Construct PortIdMap & NodePortsMap
            foreach (var port in Asset.ports)
            {
                PortIdMap.Add(port.Id, port);
                if (NodePortsMap.TryGetValue(port.belongNodeId, out var portList))
                {
                    portList.Add(port.Id);
                }
            }

            //Construct NodeValDependencyMap
            foreach (var node in Asset.nodes)
            {
                var type = node.GetType();
                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute.NodeCategory == ENodeCategory.Start)
                    StartNodeId = node.Id;

                if (!node.IsFlowNode())
                    continue;

                var valueNodeList = new List<string>();
                NodeValDependencyMap.Add(node.Id, valueNodeList);
                
                _toRunNodeList.Clear();
                _toRunNodeList.Add(node.Id);

                while (_toRunNodeList.Count > 0)
                {
                    foreach (var portId in NodePortsMap[_toRunNodeList[0]])
                    {
                        var port = PortIdMap[portId];
                        if(port.IsFlowPort() || port.direction == Direction.Output)
                            continue;
                    
                        if(string.IsNullOrEmpty(port.connectPortId))
                            continue;
                    
                        var connectPort = PortIdMap[port.connectPortId];
                        var connectNode = NodeIdMap[connectPort.belongNodeId];
                        if (!connectNode.IsValueNode()) 
                            continue;
                        valueNodeList.Add(connectNode.Id);
                        _toRunNodeList.Add(connectNode.Id);
                    }
                    
                    _toRunNodeList.RemoveAt(0);
                }
            }
        }
    }
    
    public class NodeSystemGraphRunner:MonoBehaviour
    {
        public NodeSystemGraphAsset asset;
        public readonly Dictionary<string, NodeSystemNodeRunner> NodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        public GraphAssetRuntimeData GraphAssetRuntimeData;
        private NodeSystem _nodeSystem;

        private NodeSystemFlowNodeRunner _curRunner;
        private bool _isRunning;

        #region Port Val

        public object GetInPortVal(string inPortId)
        {
            var port = GraphAssetRuntimeData.PortIdMap[inPortId];
            if (port.direction != Direction.Input)
            {
                Debug.LogWarning($"GetInPortVal failed: port {inPortId} is not input port.");
                return null;
            }
            
            if (string.IsNullOrEmpty(port.connectPortId))
                return null;
                
            inPortId = port.connectPortId;
            return _outPortResultCached.GetValueOrDefault(inPortId);
        }

        public void SetOutPortVal(string outPortId, object val)
        {
            var port = GraphAssetRuntimeData.PortIdMap[outPortId];
            if (port.direction != Direction.Output)
            {
                Debug.LogWarning($"SetOutPortVal failed: port {outPortId} is not output port.");
                return;
            }
            
            if (!_outPortResultCached.TryAdd(outPortId, val))
            {
                _outPortResultCached[outPortId] = val;
            }
        }

        #endregion
       
        
        private void Awake()
        {
            _nodeSystem = new();
            _nodeSystem.InitSystem();
            GraphAssetRuntimeData = new();
            GraphAssetRuntimeData.Init(asset);
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(GraphAssetRuntimeData.StartNodeId))
            {
                Debug.Log("No Start Node");
                return;
            }

            //Create Runner Instances
            foreach (var n in asset.nodes)
            {
                var runner = _nodeSystem.NodeRunnerFactory.CreateNodeRunner(n.GetType());
                runner.Init(n, this);
                NodeRunners.Add(n.Id, runner);
            }

            StartGraphRunner();
        }

        private void Update()
        {
            if(!_isRunning)
                return;
            
            UpdateCurNodeRunner(Time.deltaTime);
        }

        private void UpdateCurNodeRunner(float deltaTime = 0)
        {
            _curRunner.Execute(deltaTime);

            while (_curRunner.IsNodeRunnerCompleted)
            {
                var nextNode = _curRunner.GetNextNode();
                if (string.IsNullOrEmpty(nextNode))
                {
                    EndGraphRunner();
                    break;
                }
                
                _curRunner = NodeRunners[nextNode] as NodeSystemFlowNodeRunner;
                if (_curRunner == null)
                {
                    EndGraphRunner();
                    break;
                }
                _curRunner.Execute();
            }
        }
        
        private void StartGraphRunner()
        {
            Debug.Log("Start GraphRunner");
            _isRunning = true;
            _curRunner = NodeRunners[GraphAssetRuntimeData.StartNodeId] as NodeSystemFlowNodeRunner;

            UpdateCurNodeRunner();
        }
        
        private void EndGraphRunner()
        {
            Debug.Log("End GraphRunner");
            _isRunning = false;
            _curRunner = null;
        }
    }
}