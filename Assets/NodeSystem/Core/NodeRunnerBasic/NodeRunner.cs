using GameplayCommonLibrary;

namespace NS
{
    public class NodeRunner:IPoolObject
    {
        public static readonly NodeRunner DefaultRunner = new();
        protected NodeGraphRunner GraphRunner { get; private set; }
        public virtual void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            GraphRunner = graphRunner;
        }
        
        public virtual void Execute()
        {
           
        }

        #region Pool Object
        
        public virtual void OnCreateFromPool(ObjectPool pool)
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