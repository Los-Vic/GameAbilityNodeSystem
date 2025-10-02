using System;
using System.Collections.Generic;
using GCL;

namespace Gray.NG
{
    public class GraphDirector:IPoolObject
    {
        public NodeGraphManager Mgr { get; private set; }
        public RuntimeNodeGraph RuntimeNodeGraph { get; private set; }

        private readonly List<GraphExecutor> _graphExecutors = new();
        private readonly List<GraphExecutor> _tickingExecutors = new();

        public void Init(NodeGraphManager mgr, RuntimeNodeGraph runtimeNodeGraph)
        {
            Mgr = mgr;
            RuntimeNodeGraph = runtimeNodeGraph;
        }

        public void Tick(float dt)
        {
            _tickingExecutors.AddRange(_graphExecutors);
            foreach (var executor in _tickingExecutors)
            {
                executor.Tick(dt);
            }
            _tickingExecutors.Clear();
        }

        public void Start(Type entryNodeType)
        {
            var graphExecutor = CreateGraphExecutor();
            graphExecutor.Start(entryNodeType);
        }

        public void StopAll()
        {
            
        }

        private GraphExecutor CreateGraphExecutor()
        {
            var graphExecutor = Mgr.PoolCollection.Get<GraphExecutor>();
            graphExecutor.Init(this);
            _graphExecutors.Add(graphExecutor);
            return graphExecutor;
        }

        private void DestroyGraphExecutor(GraphExecutor graphExecutor)
        {
            _graphExecutors.Remove(graphExecutor);
            Mgr.PoolCollection.Release(graphExecutor);
        }
        
        public void OnCreateFromPool()
        {
            
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            foreach (var graphExecutor in _graphExecutors)
            {
                Mgr.PoolCollection.Release(graphExecutor);
            }
            _graphExecutors.Clear();
            _tickingExecutors.Clear();
            Mgr = null;
            RuntimeNodeGraph = null;
        }

        public void OnDestroy()
        {
        }
    }
}