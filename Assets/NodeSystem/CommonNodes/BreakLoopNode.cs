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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var loopNode = graphRunner.GetCurLoopNodeRunner();
            if (loopNode != null)
                loopNode.IsLoopEnd = true;
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            return graphRunner.GetCurLoopNode();
        }
    }
}