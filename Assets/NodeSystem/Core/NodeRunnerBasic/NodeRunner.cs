using GameplayCommonLibrary;

namespace NS
{
    public class NodeRunner:IPoolClass
    {
        public static readonly NodeRunner DefaultRunner = new();
        protected NodeGraphRunner GraphRunner { get; private set; }
        public string NodeId { get; private set; }
        public virtual void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            NodeId = nodeAsset.Id;
            GraphRunner = graphRunner;
        }
        
        public virtual void Execute()
        {
           
        }

        #region Pool Object
        
        public virtual void OnCreateFromPool(ClassObjectPool pool)
        {
        }

        public virtual void OnTakeFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
            NodeId = null;
            GraphRunner = null;
        }

        public virtual void OnDestroy()
        {
        }

        #endregion
    }
}