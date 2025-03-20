namespace NS
{
    [Node("BreakLoop", "Common/FlowControl/BreakLoop", ENodeFunctionType.Action, typeof(BreakLoopNodeRunner), CommonNodeCategory.FlowControl)]
    public sealed class BreakLoopNode : Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InExecPort;
    }
    
    public class BreakLoopNodeRunner:FlowNodeRunner
    {
        public override void Execute()
        {
            var loopNode = GraphRunner.GetCurLoopNode();
            if (loopNode != null)
                loopNode.IsLoopEnd = true;
        }

        public override string GetNextNode()
        {
            var loopNode = GraphRunner.GetCurLoopNode();
            return loopNode?.NodeId;
        }
    }
}