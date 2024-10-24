namespace NodeSystem
{
    public class NodeSystemNodeRunner
    {
        public static readonly NodeSystemNodeRunner DefaultRunner = new();

        public bool IsNodeRunnerCompleted { get; protected set; }
        public virtual void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            Reset();
        }

        public virtual void Reset()
        {
            IsNodeRunnerCompleted = false;
        }
        
        public virtual void Execute(float dt = 0)
        {
            
        }

        public virtual string GetNextNode()
        {
            return default;
        }
    }
}