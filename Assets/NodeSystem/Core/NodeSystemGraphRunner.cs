using System.Collections.Generic;
using CommonObjectPool;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    public class NodeSystemGraphRunner:IPoolObject
    {
        private NodeSystemGraphAsset _asset;
        private NodeSystem _nodeSystem;
        private bool _isValid;
        private readonly Dictionary<string, NodeSystemNodeRunner> _nodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        //Run node runner
        private NodeSystemNode _eventNode;
        private NodeSystemFlowNodeRunner _curRunner;
        private bool _isRunning;
        private readonly Stack<string> _runningLoopNodeIds = new();

        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        
        public void Init(NodeSystem system, NodeSystemGraphAsset asset, string eventNodeId, NodeSystemEventParamBase eventParam)
        {
            _nodeSystem = system;
            _asset = asset;
            GraphAssetRuntimeData = _nodeSystem.GetGraphRuntimeData(asset);
            _isValid = false;

            _eventNode = GraphAssetRuntimeData.GetNodeById(eventNodeId);
            if (!_eventNode.IsEventNode())
            {
                NodeSystemLogger.LogError($"Not valid event node {eventNodeId} of {asset.name}");
                return;
            }

            var eventNodeRunner = GetNodeRunner(eventNodeId) as NodeSystemEventNodeRunner;
            if (eventNodeRunner == null)
            {
                NodeSystemLogger.LogError($"Not valid event node runner {eventNodeId} of {asset.name}");
                return;
            }
            eventNodeRunner.SetUpEventParam(eventParam);
            _isValid = true;
        }

        private void DeInit()
        {
            StopRunner();
        }
        
        public void StartRunner()
        {
            if (_isRunning || !_isValid)
            {
                return;
            }
            
            NodeSystemLogger.Log($"Start graph of {_asset.name}, event {_eventNode.DisplayName()}");
            _isRunning = true;
            _curRunner = GetNodeRunner(_eventNode.Id) as NodeSystemFlowNodeRunner;

            UpdateCurNodeRunner();
        }
        
        public void StopRunner()
        {
            if (!_isRunning)
            {
                return;
            }
            
            NodeSystemLogger.Log($"Stop GraphRunner of {_asset.name}, event {_eventNode.DisplayName()}");
            _isRunning = false;
            _curRunner = null;
            _runningLoopNodeIds.Clear();
            _outPortResultCached.Clear();
            DestroyRunnerInstances();
        }
        
        public void UpdateRunner(float deltaTime = 0)
        {
            if(!_isRunning)
                return;
            
            UpdateCurNodeRunner(deltaTime);
        }

        public bool IsRunning() => _isRunning;

        public NodeSystemNodeRunner GetNodeRunner(string nodeId)
        {
            if (_nodeRunners.TryGetValue(nodeId, out var nodeRunner))
                return nodeRunner;

            var n = GraphAssetRuntimeData.GetNodeById(nodeId);
            var runner = _nodeSystem.ObjectFactory.CreateNodeRunner(n.GetType());
            runner.Init(n, this);
            _nodeRunners.Add(n.Id, runner);
            return runner;
        }
        
        
        #region Port Val

        public T GetInPortVal<T>(string inPortId)
        {
            var port = GraphAssetRuntimeData.GetPortById(inPortId);
            if (port.direction != Direction.Input)
            {
                NodeSystemLogger.LogWarning($"GetInPortVal failed: port {inPortId} is not input port.");
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
                NodeSystemLogger.LogWarning($"SetOutPortVal failed: port {outPortId} is not output port.");
                return;
            }
            
            if (!_outPortResultCached.TryAdd(outPortId, val))
            {
                _outPortResultCached[outPortId] = val;
            }
        }

        #endregion
        
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

        private void DestroyRunnerInstances()
        {
            foreach (var nodeRunners in _nodeRunners.Values)
            {
                _nodeSystem.ObjectFactory.DestroyNodeRunner(nodeRunners);
            }
            _nodeRunners.Clear();
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
                        StopRunner();
                        break;
                    }
                    nextNode = loopNode;
                }
                
                _curRunner = GetNodeRunner(nextNode) as NodeSystemFlowNodeRunner;
                if (_curRunner == null)
                {
                    StopRunner();
                    break;
                }
                _curRunner.Execute();
            }
        }

        #region Pool Object

        public void OnCreateFromPool()
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            DeInit();
        }

        public void OnDestroy()
        {
        }

        #endregion
       
    }
}