namespace NS
{
    public class NodeSystemFlowNodeRunner:NodeSystemNodeRunner
    {
        public bool IsNodeRunnerCompleted { get; protected set; }
        
        public virtual void Reset()
        {
            IsNodeRunnerCompleted = false;
        }
        
        public virtual string GetNextNode()
        {
            return default;
        }
    }
}