using System.Collections.Generic;
using DefaultNamespace;
using GameplayCommonLibrary;

namespace NS
{
    public class NodeSystem
    {
        private ObjectPoolMgr NodePoolMgr { get; set; }
        public NodeSystemObjectFactory NodeObjectFactory { get; protected set; }
        public INodeSystemTaskScheduler TaskScheduler { get; protected set; }
        
        private IGameLogger _logger;

        //Inject Logger before InitSystem
        public IGameLogger Logger
        {
            get
            {
                if (_logger == null)
                    _logger = new NodeSystemDefaultLogger();
                return _logger;
            }
            set => _logger = value;
        }
        
        private readonly Dictionary<NodeGraphAsset, GraphAssetRuntimeData> _graphAssetRuntimeDataMap = new();
        
        public virtual void InitSystem()
        {
            NodePoolMgr = new ObjectPoolMgr();
            NodeObjectFactory = new NodeSystemObjectFactory(NodePoolMgr, Logger);
            TaskScheduler = new NodeTaskScheduler(NodePoolMgr, Logger);
        }

        public virtual void ResetSystem()
        {
            NodePoolMgr.Clear();
            NodeObjectFactory.Clear();
            _graphAssetRuntimeDataMap.Clear();
            TaskScheduler.Clear();
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
        
        public GraphAssetRuntimeData GetGraphRuntimeData(NodeGraphAsset asset)
        {
            if (_graphAssetRuntimeDataMap.TryGetValue(asset, out var runtimeData))
                return runtimeData;
            
            var data = new GraphAssetRuntimeData
            {
                Logger = Logger
            };
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