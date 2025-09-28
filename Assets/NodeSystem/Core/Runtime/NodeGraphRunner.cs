using System;
using System.Collections.Generic;
using GCL;

namespace NS
{
    public enum EGraphRunnerEnd
    {
        Completed,
        Canceled,
        Aborted,
        UnInit,
    }

    public struct NodeGraphRunnerInitContext
    {
        public NodeSystem System;
        public NodeGraphAsset Asset;
        public string EntryNodeId;
        public IEntryParam EntryParam;
        public NodeGraphRunnerContext Context;
    }
    
    public abstract class NodeGraphRunnerContext
    {
    }
    
    public class NodeGraphRunner:IPoolObject
    {
        //Hook
        public Action<NodeGraphRunner, EGraphRunnerEnd> OnRunnerRunEnd;

        private NodeGraphAsset _asset;
        private NodeSystem _nodeSystem;
        private bool _isValid;
        private bool _hasStarted;
        
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        //Run node runner
        private Node _entryNode;
        private Node _curNode;
        private FlowNodeRunner _curNodeRunner;
        private readonly Dictionary<string, LoopNodeRunner> _loopNodeRunnerMap = new();
        private readonly Stack<string> _runningLoopNodeIds = new();
        
        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        public string AssetName => _asset?.name ?? "";
        public string EntryName => _entryNode?.ToString() ?? "";
        public INodeSystemTaskScheduler TaskScheduler => _nodeSystem.TaskScheduler;
        public NodeGraphRunnerContext Context { get; private set; }
        /// <summary>
        /// Graph Runner需要以一个事件节点作为起点
        /// </summary>
        public void Init(ref NodeGraphRunnerInitContext initContext)
        {
            _nodeSystem = initContext.System;
            _asset = initContext.Asset;
            GraphAssetRuntimeData = _nodeSystem.GetGraphRuntimeData(initContext.Asset);
            _entryNode = GraphAssetRuntimeData.GetNodeById(initContext.EntryNodeId);
            _curNode = _entryNode;
            Context = initContext.Context;
            
            if (!_entryNode.IsEntryNode())
            {
                GameLogger.LogError($"not valid entry node:{initContext.EntryNodeId} of {_asset.name}");
                return;
            }
            
            if (CreateNodeRunner(initContext.EntryNodeId) is not EntryNodeRunner entryNodeRunner)
            {
                GameLogger.LogError($"not valid entry node runner:{initContext.EntryNodeId} of {_asset.name}");
                return;
            }
            
            entryNodeRunner.SetEntryParam(this, _entryNode, initContext.EntryParam);
            _curNodeRunner = entryNodeRunner;
            _isValid = true;
        }

        private void UnInit()
        {
            TaskScheduler.CancelTasksOfGraphRunner(this);

            if (_curNodeRunner != null)
            {
                DestroyNodeRunner(_curNodeRunner);
                _curNodeRunner = null;
            }
            
            foreach (var runner in _loopNodeRunnerMap.Values)
            {
                DestroyNodeRunner(runner);
            }
            _loopNodeRunnerMap.Clear();
            _runningLoopNodeIds.Clear();
            _outPortResultCached.Clear();
            
            OnRunnerRunEnd = null;
            _asset = null;
            _entryNode = null;
            _curNode = null;
            _isValid = false;
            _hasStarted = false;
            GraphAssetRuntimeData = null;
            _nodeSystem = null;
            Context = null;
        }
        
        public void Start()
        {
            if (!_isValid)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode} failed. not valid.");
                return;
            }

            if (_hasStarted)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode} failed. already running.");
                return;
            }

            if (_curNodeRunner == null)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode} failed. entry runner is null.");
                return;
            }
            
            _hasStarted = true;
            GameLogger.Log($"start run graph:{_asset.name}, entry:{_entryNode}");
            Execute(_curNodeRunner, _curNode);
        }
        
        private void Complete()
        {
            GameLogger.Log($"complete graph:{_asset.name}, entry:{_entryNode}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Completed);
        }

        public void Cancel()
        {
            GameLogger.Log($"cancel graph:{_asset.name}, entry:{_entryNode}, cur:{_curNode}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Canceled);
        }

        public void Abort()
        {
            GameLogger.Log($"abort graph:{_asset.name}, entry:{_entryNode}, cur:{_curNode}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Aborted);
        }
        
        private void Execute(FlowNodeRunner nodeRunner, Node node)
        {
            if (nodeRunner == null)
            {
                Complete();
                return;
            }
            GameLogger.Log($"execute node:{node}, graph:{_asset.name}, entry:{_entryNode}");
            nodeRunner.Execute(this, node);
        }

        public void Forward()
        {
            var nextNode = _curNodeRunner.GetNextNode(this, _curNode);
            
            if (IsInLoop(out var loopNode))
            {
                if (_curNode.Id != loopNode)
                {
                    DestroyNodeRunner(_curNodeRunner);
                }
            }
            else
            {
                DestroyNodeRunner(_curNodeRunner);
            }
            
            if (!Node.IsValidNodeId(nextNode) && Node.IsValidNodeId(loopNode))
            {
                nextNode = loopNode;
            }
            
            if(Node.IsValidNodeId(nextNode))
            {
                _curNodeRunner = CreateNodeRunner(nextNode) as FlowNodeRunner;
            }
            else
            {
                _curNodeRunner = null;
            }
            
            Execute(_curNodeRunner, _curNode);
        }
        
        internal NodeRunner CreateNodeRunner(string nodeId)
        {
            var n = GraphAssetRuntimeData.GetNodeById(nodeId);
            var runner = _nodeSystem.CreateNodeRunner(n.GetType());
            runner.Init(this, n);
            return runner;
        }

        internal void DestroyNodeRunner(NodeRunner runner)
        {
            _nodeSystem.DestroyNodeRunner(runner);
        }
        
        
        #region Port Val

        public T GetInPortVal<T>(string inPortId)
        {
            var port = GraphAssetRuntimeData.GetPortById(inPortId);
            if (port.direction != EPortDirection.Input)
            {
                GameLogger.LogWarning($"GetInPortVal failed, port:{inPortId} is not input port.");
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
                GameLogger.LogWarning($"SetOutPortVal failed, port:{outPortId} is not output port.");
                return;
            }
            
            if (!_outPortResultCached.TryAdd(outPortId, val))
            {
                _outPortResultCached[outPortId] = val;
            }
        }

        #endregion
        
        #region Loop

        public void EnterLoop(LoopNodeRunner loopRunner, Node loopNode)
        {
            _runningLoopNodeIds.Push(loopNode.Id);
            _loopNodeRunnerMap.Add(loopNode.Id, loopRunner);
        }
        
        private bool IsInLoop(out string loopNodeId)
        {
            loopNodeId = _runningLoopNodeIds.Count == 0 ? null : _runningLoopNodeIds.Peek();
            return _runningLoopNodeIds.Count != 0;
        }

        public void ExitLoop()
        {
            if(_runningLoopNodeIds.Count == 0)
                return;
            var nodeId = _runningLoopNodeIds.Pop();
            _loopNodeRunnerMap.Remove(nodeId);
        }

        public LoopNodeRunner GetCurLoopNodeRunner()
        {
            var loopNodeId = GetCurLoopNode();
            return loopNodeId == null ? null : _loopNodeRunnerMap.GetValueOrDefault(loopNodeId);
        }

        public string GetCurLoopNode()
        {
            return !IsInLoop(out var loopNodeId) ? null : loopNodeId;
        }
        
        #endregion
        
        #region Pool Object

        public void OnCreateFromPool()
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
    }
}