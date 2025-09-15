using GCL;

namespace NS
{
    public struct NodeRunnerInitContext
    {
        public Node Node;
        public NodeGraphRunner GraphRunner;
    }
    
    public class NodeRunner:IPoolClass
    {
        public static readonly NodeRunner DefaultRunner = new();
        protected NodeGraphRunner GraphRunner { get; private set; }
        public string NodeId { get; private set; }
        public virtual void Init(ref NodeRunnerInitContext context)
        {
            NodeId = context.Node.Id;
            GraphRunner = context.GraphRunner;
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
            NodeId = null;
            GraphRunner = null;
        }

        public virtual void OnDestroy()
        {
        }

        #endregion
    }
}