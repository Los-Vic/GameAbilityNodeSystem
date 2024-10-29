namespace NodeSystem.Core
{
    public class NodeSystem
    {
        public NodeSystemNodeRunnerFactory NodeRunnerFactory;
        
        public virtual void InitSystem()
        {
            NodeRunnerFactory = new NodeSystemNodeRunnerFactory();
        }
        
    }
}