using GCL;

namespace Gray.NG
{
    public class NodeExecutor:IPoolObject
    {
        public GraphExecutor GraphExecutor { get; private set; }
        public RuntimeNode Node { get; private set; }
        
        public void Init(GraphExecutor graphExecutor, RuntimeNode node)
        {
            GraphExecutor = graphExecutor;
            Node = node;
        }
        
        public virtual void Tick(float dt)
        {
            
        }
        
        // public virtual async void ExecuteAsync()
        // {
        // }
        
        public virtual void Execute()
        {
        }

        public virtual void OnCreateFromPool()
        {
        }

        public virtual void OnTakeFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
            GraphExecutor = null;
            Node = null;
        }

        public virtual void OnDestroy()
        {
        }
    }
}