using MissQ;

namespace NS
{
    public enum EValueEquationType
    {
        Add,
        Multiply,
        Minus,
        DividedBy,
        Distance
    }
    
    [Node("ValueEquation", "Common/Value/ValueEquation", ENodeFunctionType.Value, typeof(ValueEquationNodeRunner), CommonNodeCategory.Value)]
    public sealed class ValueEquationNode : Node
    {
        [Exposed]
        public EValueEquationType Op;
        
        [Port(EPortDirection.Input, typeof(FP), "A")]
        public string InPortA;
        
        [Port(EPortDirection.Input, typeof(FP), "B")]
        public string InPortB;
        
        [Port(EPortDirection.Output, typeof(FP), "Res")]
        public string OutPortVal;
    }

    public sealed class ValueEquationNodeRunner : NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (ValueEquationNode)node;
            var a = graphRunner.GetInPortVal<FP>(n.InPortA);
            var b = graphRunner.GetInPortVal<FP>(n.InPortB);
            FP res = 0;

            switch (n.Op)
            {
               case EValueEquationType.Add:
                   res = a + b;
                   break;
               case EValueEquationType.Multiply:
                   res = a * b;
                   break;
               case EValueEquationType.Minus:
                   res = a - b;
                   break;
               case EValueEquationType.DividedBy:
                   res = a / b;
                   break;
               case EValueEquationType.Distance:
                   res = FP.Abs(a - b);
                   break;
            }
            graphRunner.SetOutPortVal(n.OutPortVal, res);
        }
    }

}