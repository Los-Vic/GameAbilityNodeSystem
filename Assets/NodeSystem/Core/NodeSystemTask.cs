namespace NS
{
    public abstract class NodeSystemTaskContext
    {
    }
    
    public interface INodeSystemTask
    {
        public int GetPriority();
        public void StartTask();
        public void EndTask();
        public void CancelTask();
    }
}