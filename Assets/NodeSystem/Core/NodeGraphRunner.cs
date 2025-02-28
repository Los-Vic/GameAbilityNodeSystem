using System;
using System.Collections.Generic;
using GameplayCommonLibrary;

namespace NS
{
    public enum EGraphRunnerEnd
    {
        Completed,
        Canceled,
        UnInit,
    }
    
    public abstract class NodeGraphRunnerContext
    {
    }
    
    public class NodeGraphRunner:IPoolObject, IRefCountRequester
    {
        private NodeGraphAsset _asset;
        private NodeSystem _nodeSystem;
        private bool _isValid;
        private readonly Dictionary<string, NodeRunner> _nodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        //Run node runner
        private Node _portalNode;
        private FlowNodeRunner _curNodeRunner;
        private readonly Stack<string> _runningLoopNodeIds = new();

        private Action<NodeGraphRunner, EGraphRunnerEnd> _onRunnerRunEnd;
        private Action<NodeGraphRunner> _onRunnerDestroy;
        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        public string AssetName => _asset?.name ?? "";
        public string PortalName => _portalNode?.DisplayName() ?? "";
        public string PortalNodeId => _portalNode?.Id ?? "";
        public INodeSystemTaskScheduler TaskScheduler => _nodeSystem.TaskScheduler;

        public NodeGraphRunnerContext Context { get; private set; }
        /// <summary>
        /// Graph Runner需要以一个事件节点作为起点
        /// </summary>
        public void Init(NodeSystem system, NodeGraphAsset asset, string portalNodeId, IPortalParam actionStartParam
        , Action<NodeGraphRunner, EGraphRunnerEnd> onRunnerRunEnd, Action<NodeGraphRunner> onRunnerDestroy = null, NodeGraphRunnerContext context = null)
        {
            _nodeSystem = system;
            _asset = asset;
            GraphAssetRuntimeData = _nodeSystem.GetGraphRuntimeData(asset);
            _isValid = false;
            _onRunnerRunEnd = onRunnerRunEnd;
            _onRunnerDestroy = onRunnerDestroy;
            _portalNode = GraphAssetRuntimeData.GetNodeById(portalNodeId);
            Context = context;
            
            if (!_portalNode.IsPortalNode())
            {
                GameLogger.LogError($"not valid portal node {portalNodeId} of {asset.name}");
                return;
            }

            var portalNodeRunner = GetNodeRunner(portalNodeId) as PortalNodeRunner;
            if (portalNodeRunner == null)
            {
                GameLogger.LogError($"not valid portal node runner {portalNodeId} of {asset.name}");
                return;
            }
            portalNodeRunner.SetPortalParam(actionStartParam);
            _isValid = true;
        }

        private void UnInit()
        {
            _onRunnerDestroy?.Invoke(this);
            _onRunnerDestroy = null;
            
            TaskScheduler.CancelTasksOfGraphRunner(this);
            Clear();
            
            _onRunnerRunEnd = null;
            _asset = null;
            _portalNode = null;
            _isValid = false;
            GraphAssetRuntimeData = null;
            _nodeSystem = null;
            Context = null;
        }

        /// <summary>
        /// 每次重跑前的清理
        /// </summary>
        private void Clear()
        {
            _curNodeRunner = null;
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
                GameLogger.LogError($"start run graph {_asset.name}, portal {_portalNode.DisplayName()} failed. not valid.");
                return;
            }

            if (IsRunning())
            {
                GameLogger.LogError($"start run graph {_asset.name}, portal {_portalNode.DisplayName()} failed. already running.");
                return;
            }
            
            GameLogger.Log($"start run graph {_asset.name}, portal {_portalNode.DisplayName()}");
            _curNodeRunner = GetNodeRunner(_portalNode.Id) as FlowNodeRunner;
            ExecuteRunner();
        }
        
        private void CompleteRunner()
        {
            GameLogger.Log($"complete graph {_asset.name}, portal {_portalNode.DisplayName()}");
            _onRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Completed);

            Clear();
        }

        public void CancelRunner()
        {
            GameLogger.Log($"cancel graph {_asset.name}, portal {_portalNode.DisplayName()}");
            _onRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Canceled);
            
            Clear();
        }
        
        internal void ExecuteRunner()
        {
            if (_curNodeRunner == null)
            {
                CompleteRunner();
                return;
            }
            GameLogger.Log($"execute node {_curNodeRunner.GetType()}, graph {_asset.name}, portal {_portalNode.DisplayName()}");
            _curNodeRunner.Execute();
        }

        internal void MoveToNextNode()
        {
            if (IsInLoop(out var loopNode) && GetNodeRunner(loopNode) != _curNodeRunner)
            {
                _curNodeRunner.Reset();
            }
                
            var nextNode = _curNodeRunner.GetNextNode();
            if (!Node.IsValidNodeId(nextNode))
            {
                if (!Node.IsValidNodeId(loopNode))
                {
                    _curNodeRunner = null;
                    return;
                }
                nextNode = loopNode;
            }
                
            _curNodeRunner = GetNodeRunner(nextNode) as FlowNodeRunner;
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
            if (port.direction != EPortDirection.Input)
            {
                GameLogger.LogWarning($"GetInPortVal failed: port {inPortId} is not input port.");
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
            if (port.direction != EPortDirection.Output)
            {
                GameLogger.LogWarning($"SetOutPortVal failed: port {outPortId} is not output port.");
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

        public void OnCreateFromPool(ObjectPool pool)
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            UnInit();
        }

        public void OnDestroy()
        {
        }

        #endregion

        public bool IsRequesterStillValid()
        {
            return _isValid;
        }

        public string GetRequesterDesc()
        {
            return $"graph:{_asset.name}, portal:{_portalNode.DisplayName()}";
        }
    }
}