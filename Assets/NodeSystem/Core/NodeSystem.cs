using System.Collections.Generic;
using CommonObjectPool;

namespace NS
{
    public class NodeSystem
    {
        private ObjectPoolMgr NodePoolMgr { get; set; }
        public NodeSystemObjectFactory NodeObjectFactory { get; protected set; }
        public INodeSystemTaskScheduler TaskScheduler { get; protected set; }
        
        private readonly Dictionary<NodeGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        
        public virtual void InitSystem()
        {
            NodePoolMgr = new ObjectPoolMgr();
            NodeObjectFactory = new NodeSystemObjectFactory(NodePoolMgr);
            TaskScheduler = new NodeTaskScheduler(NodePoolMgr);
        }

        public virtual void UnInitSystem()
        {
            NodePoolMgr.Clear();
            NodeObjectFactory.Clear();
            _graphAssetRuntimeDataMap.Clear();
        }

        public virtual void UpdateSystem(float dt)
        {
            TaskScheduler.UpdateScheduler(dt);
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