using GCL;

namespace Gray.NG
{
    [Node(ExecutorType = typeof(EntryNodeExecutor))]
    public class RtEntryNode:RuntimeNode
    {
        [InputPort(DisplayName = "Val", ConnectorStyle = EPortConnectorStyle.Circle)]
        public int EntryVal;

        [OutputPort(ConnectorStyle = EPortConnectorStyle.Arrowhead)]
        public RuntimeNode NextNode;
    }
    
    
    public class EntryNodeExecutor:NodeExecutor
    {
        public override void Execute()
        {
            GameLogger.Log("Execute entry node!");
            GraphExecutor.Forward(((RtEntryNode)Node).NextNode);
        }
    }
}