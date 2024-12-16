using System;
using System.Collections.Generic;
using CommonObjectPool;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    public enum EGraphRunnerEnd
    {
        Completed,
        Canceled,
    }
    
    public class NodeGraphRunner:IPoolObject
    {
        private NodeGraphAsset _asset;
        private NodeSystem _nodeSystem;
        private bool _isValid;
        private readonly Dictionary<string, NodeRunner> _nodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        //Run node runner
        private Node _eventNode;
        private FlowNodeRunner _curRunner;
        private readonly Stack<string> _runningLoopNodeIds = new();

        private Action<NodeGraphRunner, EGraphRunnerEnd> _onRunnerRunEnd;
        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        public string AssetName => _asset?.name ?? "";
        public string EventName => _eventNode?.DisplayName() ?? "";
        
        public INodeSystemTaskScheduler TaskScheduler => _nodeSystem.TaskScheduler;

        /// <summary>
        /// Graph Runner需要以一个事件节点作为起点
        /// </summary>
        public void Init(NodeSystem system, NodeGraphAsset asset, string eventNodeId, NodeSystemEventParamBase eventParam
        , Action<NodeGraphRunner, EGraphRunnerEnd> onRunnerRunEnd)
        {
            _nodeSystem = system;
            _asset = asset;
            GraphAssetRuntimeData = _nodeSystem.GetGraphRuntimeData(asset);
            _isValid = false;
            _onRunnerRunEnd = onRunnerRunEnd;
            _eventNode = GraphAssetRuntimeData.GetNodeById(eventNodeId);
            if (!_eventNode.IsEventNode())
            {
                NodeSystemLogger.LogError($"not valid event node {eventNodeId} of {asset.name}");
                return;
            }

            var eventNodeRunner = GetNodeRunner(eventNodeId) as EventNodeRunner;
            if (eventNodeRunner == null)
            {
                NodeSystemLogger.LogError($"not valid event node runner {eventNodeId} of {asset.name}");
                return;
            }
            eventNodeRunner.SetUpEventParam(eventParam);
            _isValid = true;
        }

        private void DeInit()
        {
            TaskScheduler.CancelTasksOfGraphRunner(this);
            _onRunnerRunEnd = null;
            _asset = null;
            _eventNode = null;
            _isValid = false;
            GraphAssetRuntimeData = null;
            _nodeSystem = null;
            Clear();
        }

        private void Clear()
        {
            _curRunner = null;
            _runningLoopNodeIds.Clear();
            _outPortResultCached.Clear();
            DestroyRunnerInstances();
        }

        private bool IsRunning()
        {
            return _nodeSystem.TaskScheduler.HasTaskRunning(this);
        }
        
        public void StartRunner()
        {
            if (!_isValid)
            {
                NodeSystemLogger.LogError($"start graph of {_asset.name} failed. not valid.");
                return;
            }

            if (IsRunning())
            {
                NodeSystemLogger.LogError($"start graph of {_asset.name} failed. already running.");
                return;
            }
            
            NodeSystemLogger.Log($"start graph of {_asset.name}, event:{_eventNode.DisplayName()}");
            _curRunner = GetNodeRunner(_eventNode.Id) as FlowNodeRunner;
            ExecuteRunner();
        }
        
        private void CompleteRunner()
        {
            NodeSystemLogger.Log($"complete graph of {_asset.name}, event:{_eventNode.DisplayName()}");
            _onRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Completed);

            Clear();
        }

        public void CancelRunner()
        {
            NodeSystemLogger.Log($"cancel graph of {_asset.name}, event:{_eventNode.DisplayName()}");
            _onRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Canceled);
            
            Clear();
        }
        
        internal void ExecuteRunner()
        {
            if (_curRunner == null)
            {
                CompleteRunner();
                return;
            }
            _curRunner.Execute();
        }

        internal void MoveToNextNode()
        {
            if (IsInLoop(out var loopNode) && GetNodeRunner(loopNode) != _curRunner)
            {
                _curRunner.Reset();
            }
                
            var nextNode = _curRunner.GetNextNode();
            if (!Node.IsValidNodeId(nextNode))
            {
                if (!Node.IsValidNodeId(loopNode))
                {
                    _curRunner = null;
                    return;
                }
                nextNode = loopNode;
            }
                
            _curRunner = GetNodeRunner(nextNode) as FlowNodeRunner;
        }
        
        public NodeRunner GetNodeRunner(string nodeId)
        {
            if (_nodeRunners.TryGetValue(nodeId, out var nodeRunner))
                return nodeRunner;

            var n = GraphAssetRuntimeData.GetNodeById(nodeId);
            var runner = _nodeSystem.NodeObjectFactory.CreateNodeRunner(n.GetType());
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
            
            if (!NodePort.IsValidPortId(port.connectPortId))
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
        
        private bool IsInLoop(out string loopNodeId)
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
                _nodeSystem.NodeObjectFactory.DestroyNodeRunner(nodeRunners);
            }
            _nodeRunners.Clear();
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