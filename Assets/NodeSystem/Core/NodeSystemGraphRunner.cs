using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    public class GraphAssetRuntimeData
    {
        public NodeSystemGraphAsset Asset { get; private set; }
        private readonly Dictionary<string, NodeSystemNode> _nodeIdMap = new();
        private readonly Dictionary<string, NodeSystemPort> _portIdMap = new();
        private readonly Dictionary<string, List<string>> _nodePortsMap = new();
        //To execute flow node, we need output value of dependent value nodes
        private readonly Dictionary<string, List<string>> _nodeValDependencyMap = new();
        public string StartNodeId { get;private set; }
        
        private readonly List<string> _toRunNodeList = new();

        public void Init(NodeSystemGraphAsset asset)
        {
            Asset = asset;
            
            //Construct NodeIdMap & NodePortsMap
            foreach (var node in Asset.nodes)
            {
                _nodeIdMap.Add(node.Id, node);
                _nodePortsMap.Add(node.Id, new List<string>());
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
                var type = node.GetType();
                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute.NodeCategory == ENodeCategory.Start)
                    StartNodeId = node.Id;

                if (!node.IsFlowNode())
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
                        if(port.IsFlowPort() || port.direction == Direction.Output)
                            continue;
                    
                        if(!NodeSystemPort.IsValidPortId(port.connectPortId))
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

        public NodeSystemNode GetNodeById(string id) => _nodeIdMap.GetValueOrDefault(id);
        public NodeSystemPort GetPortById(string id) => _portIdMap.GetValueOrDefault(id);
        public List<string> GetPortIdsOfNode(string nodeId) => _nodePortsMap.GetValueOrDefault(nodeId, new List<string>());
        public List<string> GetDependentNodeIds(string nodeId) => _nodeValDependencyMap.GetValueOrDefault(nodeId, new List<string>());
    }
    
    public class NodeSystemGraphRunner:MonoBehaviour
    {
        public NodeSystemGraphAsset asset;
        private readonly Dictionary<string, NodeSystemNodeRunner> _nodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        private NodeSystem _nodeSystem;

        private NodeSystemFlowNodeRunner _curRunner;
        private bool _isRunning;
        private readonly Stack<string> _runningLoopNodeIds = new();

        #region Port Val

        public T GetInPortVal<T>(string inPortId)
        {
            var port = GraphAssetRuntimeData.GetPortById(inPortId);
            if (port.direction != Direction.Input)
            {
                Debug.LogWarning($"GetInPortVal failed: port {inPortId} is not input port.");
                return default;
            }
            
            if (!NodeSystemPort.IsValidPortId(port.connectPortId))
                return default;
                
            inPortId = port.connectPortId;
            return (T)_outPortResultCached.GetValueOrDefault(inPortId);
        }

        public void SetOutPortVal(string outPortId, object val)
        {
            var port = GraphAssetRuntimeData.GetPortById(outPortId);
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
       
        public NodeSystemNodeRunner GetNodeRunner(string nodeId) => _nodeRunners.GetValueOrDefault(nodeId, NodeSystemNodeRunner.DefaultRunner);

        #region Loop

        public void EnterLoop(string nodeId)
        {
            _runningLoopNodeIds.Push(nodeId);
        }
        
        public bool IsInLoop(out string loopNodeId)
        {
            loopNodeId = _runningLoopNodeIds.Count == 0 ? default : _runningLoopNodeIds.Peek();
            return _runningLoopNodeIds.Count != 0;
        }

        public void ExitLoop()
        {
            if(_runningLoopNodeIds.Count == 0)
                return;
            _runningLoopNodeIds.Pop();
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
            if (!NodeSystemNode.IsValidNodeId(GraphAssetRuntimeData.StartNodeId))
            {
                Debug.Log("No Start Node");
                return;
            }

            //Create Runner Instances
            foreach (var n in asset.nodes)
            {
                var runner = _nodeSystem.NodeRunnerFactory.CreateNodeRunner(n.GetType());
                runner.Init(n, this);
                _nodeRunners.Add(n.Id, runner);
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

            while (_curRunner.IsCompleted())
            {
                if (IsInLoop(out var loopNode) && GetNodeRunner(loopNode) != _curRunner)
                {
                    _curRunner.Reset();
                }
                
                var nextNode = _curRunner.GetNextNode();
                if (!NodeSystemNode.IsValidNodeId(nextNode))
                {
                    if (!NodeSystemNode.IsValidNodeId(loopNode))
                    {
                        EndGraphRunner();
                        break;
                    }
                    nextNode = loopNode;
                }
                
                _curRunner = _nodeRunners[nextNode] as NodeSystemFlowNodeRunner;
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
            _curRunner = _nodeRunners[GraphAssetRuntimeData.StartNodeId] as NodeSystemFlowNodeRunner;

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