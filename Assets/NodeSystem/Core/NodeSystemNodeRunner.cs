using NodeSystem.ObjectPool;

namespace NodeSystem.Core
{
    public class NodeSystemNodeRunner:IPoolObject
    {
        public static readonly NodeSystemNodeRunner DefaultRunner = new();
        
        public virtual void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            
        }
        
        public virtual void Execute(float dt = 0)
        {
           
        }

        #region Pool Object

        public ObjectPoolParam GetPoolParam()
        {
            return new ObjectPoolParam()
            {
                Capacity = 64,
                MaxSize = 128
            };
        }

        public virtual void OnCreateFromPool()
        {
        }

        public virtual void OnTakeFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
        }

        public virtual void OnDestroy()
        {
        }

        #endregion
        
    }
}