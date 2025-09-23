namespace NS
{
    [Node("Bool", "Common/Value/Bool", ENodeFunctionType.Value ,typeof(BoolNodeRunner), CommonNodeCategory.Value)]
    public sealed class BoolNode : Node
    {
        [Exposed] public bool Val;

        [Port(EPortDirection.Output, typeof(bool))] 
        public string OutPortVal;
    }
    
    public sealed class BoolNodeRunner:NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (BoolNode)node;
            graphRunner.SetOutPortVal(n.OutPortVal, n.Val);
        }
    }
}