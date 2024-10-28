namespace NodeSystem
{
    public class NodeSystemNodeRunner
    {
        public static readonly NodeSystemNodeRunner DefaultRunner = new();

        public bool IsNodeRunnerCompleted { get; protected set; }
        public virtual void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            
        }

        public virtual void Reset()
        {
            IsNodeRunnerCompleted = false;
        }
        
        public virtual void Execute(float dt = 0)
        {
            IsNodeRunnerCompleted = true;
        }

        public virtual string GetNextNode()
        {
            return default;
        }
    }
}