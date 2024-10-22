namespace NodeSystem
{
    public class NodeSystem
    {
        private NodeSystemNodeRunnerFactory _nodeRunnerFactory;
        
        public virtual void InitSystem()
        {
            _nodeRunnerFactory = new NodeSystemNodeRunnerFactory();
            
        }
        
    }
}