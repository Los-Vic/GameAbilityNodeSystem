namespace Gray.NG
{
    public abstract class NodeExecutor
    {
        public virtual async void ExecuteAsync(NodeGraphDirector director, RuntimeNode node)
        {
            
        }

        public virtual void Execute(NodeGraphDirector director, RuntimeNode node)
        {
            
        }
    }
}