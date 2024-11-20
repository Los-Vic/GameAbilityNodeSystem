using System.Collections.Generic;

namespace NS
{
    public class NodeSystem
    {
        public NodeSystemObjectFactory NodeObjectFactory { get; protected set; }
        public NodeSystemTaskScheduler TaskScheduler { get; protected set; }
        private readonly Dictionary<NodeSystemGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        
        public virtual void InitSystem()
        {
            NodeObjectFactory = new NodeSystemObjectFactory();
            TaskScheduler = new NodeSystemTaskScheduler();
        }

        public virtual void UnInitSystem()
        {
            NodeObjectFactory.Clear();
            _graphAssetRuntimeDataMap.Clear();
        }

        public GraphAssetRuntimeData GetGraphRuntimeData(NodeSystemGraphAsset asset)
        {
            if (_graphAssetRuntimeDataMap.TryGetValue(asset, out var runtimeData))
                return runtimeData;
            
            var data = new GraphAssetRuntimeData();
            data.Init(asset);
            _graphAssetRuntimeDataMap.Add(asset, data);
            return data;
        }
    }
}