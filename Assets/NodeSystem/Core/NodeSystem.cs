﻿using System.Collections.Generic;
using GameplayCommonLibrary;

namespace NS
{
    public class NodeSystem
    {
        private ClassObjectPoolMgr NodePoolMgr { get; set; }
        public NodeSystemObjectFactory NodeObjectFactory { get; protected set; }
        public INodeSystemTaskScheduler TaskScheduler { get; protected set; }
        
        private readonly Dictionary<NodeGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();

        public virtual void OnCreateSystem()
        {
            NodePoolMgr = new ClassObjectPoolMgr();
            NodeObjectFactory = new NodeSystemObjectFactory(NodePoolMgr);
            TaskScheduler = new NodeTaskScheduler(NodePoolMgr);
        }
        
        public virtual void InitSystem()
        {
          
        }
        
        public virtual void UnInitSystem()
        {
            NodePoolMgr.Clear();
            NodeObjectFactory.Clear();
            _graphAssetRuntimeDataMap.Clear();
            TaskScheduler.Clear();
        }

        public virtual void UpdateSystem(float dt)
        {
            TaskScheduler.UpdateScheduler(dt);
        }

        public virtual void OnDestroySystem()
        {
            
        }
        
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