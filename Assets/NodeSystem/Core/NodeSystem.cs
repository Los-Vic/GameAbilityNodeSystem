using System;
using System.Collections.Generic;
using System.Reflection;
using GameplayCommonLibrary;

namespace NS
{
    public class NodeSystem
    {
        #region Class Reference

        public INodeSystemTaskScheduler TaskScheduler { get; private set; }
        public ClassObjectPoolMgr NodePoolMgr { get; private set; }

        #endregion
        
        
        #region Essential States

        #endregion
     
        
        #region Accidental States

        private readonly Dictionary<NodeGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        private readonly Dictionary<Type, Type> _cachedNodeToNodeRunnerTypeMap = new();

        #endregion
        
        public virtual void OnCreateSystem()
        {
            NodePoolMgr = new ClassObjectPoolMgr();
            TaskScheduler = CreateTaskScheduler();
        }
        
        public virtual void InitSystem()
        {
          
        }
        
        public virtual void UnInitSystem()
        {
            NodePoolMgr.Clear();
            TaskScheduler.Clear();
            _cachedNodeToNodeRunnerTypeMap.Clear();
            _graphAssetRuntimeDataMap.Clear();
        }

        public virtual void UpdateSystem(float dt)
        {
            TaskScheduler.UpdateScheduler(dt);
        }
        
        protected virtual INodeSystemTaskScheduler CreateTaskScheduler()
        {
            return new NodeTaskScheduler(NodePoolMgr);
        }

        #region Graph Runner

        public virtual NodeGraphRunner CreateGraphRunner()
        {
            return NodePoolMgr.Get<NodeGraphRunner>() ;
        }

        public virtual void DestroyGraphRunner(NodeGraphRunner runner)
        {
            NodePoolMgr.Release(runner);
        }

        #endregion

        #region Node Runner

        public virtual NodeRunner CreateNodeRunner(Type type)
        {
            if (!_cachedNodeToNodeRunnerTypeMap.TryGetValue(type, out var runnerType))
            {
                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute == null)
                {
                    return NodeRunner.DefaultRunner;
                }

                runnerType = nodeAttribute.NodeRunnerType;
                if (runnerType == null)
                {
                    return NodeRunner.DefaultRunner;
                }
                _cachedNodeToNodeRunnerTypeMap.Add(type, runnerType);
            }

            var runner = NodePoolMgr.Get(runnerType) as NodeRunner;
            return runner ?? NodeRunner.DefaultRunner;
        }

        public virtual void DestroyNodeRunner(NodeRunner runner)
        {
            if(runner == NodeRunner.DefaultRunner)
                return;
            NodePoolMgr.Release(runner);
        }

        #endregion
      
        
        public GraphAssetRuntimeData GetGraphRuntimeData(NodeGraphAsset asset)
        {
            if (_graphAssetRuntimeDataMap.TryGetValue(asset, out var runtimeData))
                return runtimeData;

            var data = new GraphAssetRuntimeData();
            data.Init(asset);
            _graphAssetRuntimeDataMap.Add(asset, data);
            return data;
        }
        
        public virtual void DumpObjectPool()
        {
            NodePoolMgr.Log();
        }
    }
}