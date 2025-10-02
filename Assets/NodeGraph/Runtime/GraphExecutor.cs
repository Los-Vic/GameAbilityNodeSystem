using System;
using System.Reflection;
using GCL;

namespace Gray.NG
{
    public class GraphExecutor:IPoolObject
    {
        public GraphDirector Director { get; private set; }
        public NodeExecutor CurExecutor { get; private set; }

        private Type _entryNodeType;
        
        public void Init(GraphDirector director)
        {
            Director = director;
        }

        public void Tick(float dt)
        {
            CurExecutor?.Tick(dt);
        }

        public void Start(Type entryNodeType)
        {
            _entryNodeType = entryNodeType;
            RuntimeNode entryNode = null;
            foreach (var i in Director.RuntimeNodeGraph.entryNodeIndexed)
            {
                var node = Director.RuntimeNodeGraph.nodes[i];
                if (node.GetType() == entryNodeType)
                {
                    entryNode = node;
                    break;
                }
            }

            if (entryNode == null)
            {
                GameLogger.LogError($"Failed to find entry node, type={_entryNodeType}");
                return;
            }
            
            CurExecutor = CreateNodeExecutor(entryNode);
            CurExecutor.Execute();
        }

        public void Forward(RuntimeNode node)
        {
            if (node == null)
            {
                GameLogger.Log(
                    $"Graph executor normal end, entry node={_entryNodeType}, end node={CurExecutor.Node.GetType()}");
                
                DestroyNodeExecutor(CurExecutor);
                CurExecutor = null;
                return;
            }
            
            DestroyNodeExecutor(CurExecutor);
            CurExecutor = CreateNodeExecutor(node);
            CurExecutor.Execute();
        }
        
        private NodeExecutor CreateNodeExecutor(RuntimeNode node)
        {
            var t = node.GetType().GetCustomAttribute<NodeAttribute>().ExecutorType;
            var executor = (NodeExecutor)Director.Mgr.PoolCollection.Get(t);
            executor.Init(this, node);
            return executor;
        }

        private void DestroyNodeExecutor(NodeExecutor executor)
        {
            Director.Mgr.PoolCollection.Release(executor);
        }
        
        public void OnCreateFromPool()
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            Director = null;
        }

        public void OnDestroy()
        {
        }
    }
}