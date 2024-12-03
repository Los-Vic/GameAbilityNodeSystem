using CommonObjectPool;

namespace NS
{
    public class NodeRunner:IPoolObject
    {
        public static readonly NodeRunner DefaultRunner = new();
        
        public virtual void Init(Node nodeAsset, NodeGraphRunner graphRunner)
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