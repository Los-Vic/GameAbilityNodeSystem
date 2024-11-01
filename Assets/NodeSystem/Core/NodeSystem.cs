using System.Collections.Generic;

namespace NS
{
    public class NodeSystem
    {
        public NodeSystemObjectFactory ObjectFactory;
        private readonly Dictionary<NodeSystemGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        
        public virtual void InitSystem()
        {
            ObjectFactory = new NodeSystemObjectFactory();
        }

        public virtual void DeInitSystem()
        {
            ObjectFactory.Clear();
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