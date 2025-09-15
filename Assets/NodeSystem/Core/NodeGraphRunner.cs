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
    
    public class NodeGraphRunner:IPoolClass
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
        private FlowNodeRunner _curNodeRunner;
        private readonly Dictionary<string, LoopNodeRunner> _loopNodeRunnerMap = new();
        private readonly Stack<string> _runningLoopNodeIds = new();
        
        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        public string AssetName => _asset?.name ?? "";
        public string EntryName => _entryNode?.DisplayName() ?? "";
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
            entryNodeRunner.SetEntryParam(initContext.EntryParam);
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
            _isValid = false;
            _hasStarted = false;
            GraphAssetRuntimeData = null;
            _nodeSystem = null;
            Context = null;
        }
        
        public void StartRunner()
        {
            if (!_isValid)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode.DisplayName()} failed. not valid.");
                return;
            }

            if (_hasStarted)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode.DisplayName()} failed. already running.");
                return;
            }

            if (_curNodeRunner == null)
            {
                GameLogger.LogError($"start run graph:{_asset.name}, entry:{_entryNode.DisplayName()} failed. entry runner is null.");
                return;
            }
            
            _hasStarted = true;
            GameLogger.Log($"start run graph:{_asset.name}, entry:{_entryNode.DisplayName()}");
            ExecuteRunner();
        }
        
        private void CompleteRunner()
        {
            GameLogger.Log($"complete graph:{_asset.name}, entry:{_entryNode.DisplayName()}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Completed);
        }

        public void CancelRunner()
        {
            GameLogger.Log($"cancel graph:{_asset.name}, entry:{_entryNode.DisplayName()}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Canceled);
        }

        public void AbortRunner()
        {
            GameLogger.Log($"abort graph:{_asset.name}, entry:{_entryNode.DisplayName()}");
            OnRunnerRunEnd?.Invoke(this, EGraphRunnerEnd.Aborted);
        }
        
        private void ExecuteRunner()
        {
            if (_curNodeRunner == null)
            {
                CompleteRunner();
                return;
            }
            GameLogger.Log($"execute node:{_curNodeRunner.GetType()}, graph:{_asset.name}, entry:{_entryNode.DisplayName()}");
            _curNodeRunner.Execute();
        }

        internal void ForwardRunner()
        {
            var nextNode = _curNodeRunner.GetNextNode();
            
            if (IsInLoop(out var loopNode))
            {
                if (_curNodeRunner.NodeId != loopNode)
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
            
            ExecuteRunner();
        }
        
        internal NodeRunner CreateNodeRunner(string nodeId)
        {
            var n = GraphAssetRuntimeData.GetNodeById(nodeId);
            var runner = _nodeSystem.CreateNodeRunner(n.GetType());
            var context = new NodeRunnerInitContext()
            {
                Node = n,
                GraphRunner = this,
            };
            runner.Init(ref context);
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

        public void EnterLoop(LoopNodeRunner loopRunner)
        {
            _runningLoopNodeIds.Push(loopRunner.NodeId);
            _loopNodeRunnerMap.Add(loopRunner.NodeId, loopRunner);
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

        public LoopNodeRunner GetCurLoopNode()
        {
            if (!IsInLoop(out var loopNodeId)) 
                return null;
            
            return _loopNodeRunnerMap.GetValueOrDefault(loopNodeId);
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