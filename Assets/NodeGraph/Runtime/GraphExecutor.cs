using System;
using System.Reflection;
using Gameplay.Common;

namespace Gray.NG
{
    public enum EGraphEnd
    {
        Completed,
        Failed,
        Cancelled,
    }
    
    public class GraphExecutor:IPoolObject
    {
        public GraphDirector Director { get; private set; }
        public NodeExecutor CurExecutor { get; private set; }

        private Type _entryNodeType;
        private Action<GraphExecutor, EGraphEnd> _onEndDelegate;
        
        public void Init(GraphDirector director, Action<GraphExecutor, EGraphEnd> onEnd)
        {
            Director = director;
            _onEndDelegate = onEnd;
        }

        public void Tick(float dt)
        {
            
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
                End(EGraphEnd.Completed);
                return;
            }
            
            DestroyNodeExecutor(CurExecutor);
            CurExecutor = CreateNodeExecutor(node);
            CurExecutor.Execute();
        }

        public void Stop()
        {
            End(EGraphEnd.Cancelled);
        }

        public void Abort()
        {
            End(EGraphEnd.Failed);
        }

        private void End(EGraphEnd endType)
        {
            GameLogger.Log(
                $"Graph executor end with type={endType}, entry node={_entryNodeType}, end node={CurExecutor?.Node.GetType()}");
            
            _onEndDelegate?.Invoke(this, endType);
            if(CurExecutor == null)
                return;
            
            DestroyNodeExecutor(CurExecutor);
            CurExecutor = null;
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

        #region IPoolObject

        public void OnCreateFromPool()
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            Director = null;
            CurExecutor = null;
            _onEndDelegate = null;
        }

        public void OnDestroy()
        {
        }

        #endregion
       
    }
}