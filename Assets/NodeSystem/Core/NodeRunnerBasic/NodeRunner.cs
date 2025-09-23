using GCL;

namespace NS
{
    public class NodeRunner:IPoolObject
    {
        public static readonly NodeRunner DefaultRunner = new();
        
        public virtual void Init(NodeGraphRunner graphRunner, Node node)
        {
        }
        
        public virtual void Execute(NodeGraphRunner graphRunner, Node node)
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