using CommonObjectPool;

namespace NS
{
    public class NodeSystemNodeRunner:IPoolObject
    {
        public static readonly NodeSystemNodeRunner DefaultRunner = new();
        
        public virtual void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            
        }
        
        public virtual void Execute()
        {
           
        }

        #region Pool Object
        
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