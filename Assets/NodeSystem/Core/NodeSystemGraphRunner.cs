﻿using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    public class NodeSystemGraphRunner:IPoolObject
    {
        private NodeSystemGraphAsset _asset;
        private NodeSystem _nodeSystem;
        private readonly Dictionary<string, NodeSystemNodeRunner> _nodeRunners = new();
        //Cache value of node output 
        private readonly Dictionary<string, object> _outPortResultCached = new();
        
        //Run node runner
        private NodeSystemFlowNodeRunner _curRunner;
        private bool _isRunning;
        private readonly Stack<string> _runningLoopNodeIds = new();

        public GraphAssetRuntimeData GraphAssetRuntimeData { get; private set; }
        
        public void Init(NodeSystem system, NodeSystemGraphAsset asset)
        {
            _nodeSystem = system;
            _asset = asset;
            GraphAssetRuntimeData = _nodeSystem.GetGraphRuntimeData(asset);
            CreateRunnerInstances();
        }

        private void DeInit()
        {
            DestroyRunnerInstances();
        }

        public void StartRunner()
        {
            if (!NodeSystemNode.IsValidNodeId(GraphAssetRuntimeData.StartNodeId))
            {
                Debug.Log("No Start Node");
                return;
            }
            StartGraphRunner();
        }

        public void UpdateRunner(float deltaTime = 0)
        {
            if(!_isRunning)
                return;
            
            UpdateCurNodeRunner(deltaTime);
        }

        public void StopRunner()
        {
            EndGraphRunner();
        }
        
        public NodeSystemNodeRunner GetNodeRunner(string nodeId) => _nodeRunners.GetValueOrDefault(nodeId, NodeSystemNodeRunner.DefaultRunner);
        
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
        
        private void CreateRunnerInstances()
        {
            foreach (var n in _asset.nodes)
            {
                var runner = _nodeSystem.RunnerFactory.CreateNodeRunner(n.GetType());
                runner.Init(n, this);
                _nodeRunners.Add(n.Id, runner);
            }
        }

        private void DestroyRunnerInstances()
        {
            foreach (var nodeRunners in _nodeRunners.Values)
            {
                _nodeSystem.RunnerFactory.DestroyNodeRunner(nodeRunners);
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
            Debug.Log($"Start GraphRunner of {_asset.name}");
            _isRunning = true;
            _curRunner = _nodeRunners[GraphAssetRuntimeData.StartNodeId] as NodeSystemFlowNodeRunner;

            UpdateCurNodeRunner();
        }
        
        private void EndGraphRunner()
        {
            Debug.Log($"End GraphRunner of {_asset.name}");
            _isRunning = false;
            _curRunner = null;
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