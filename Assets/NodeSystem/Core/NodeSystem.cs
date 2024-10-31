using System.Collections.Generic;

namespace NS
{
    public class NodeSystem
    {
        public NodeSystemRunnerFactory RunnerFactory;
        private readonly Dictionary<NodeSystemGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        
        public virtual void InitSystem()
        {
            RunnerFactory = new NodeSystemRunnerFactory();
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