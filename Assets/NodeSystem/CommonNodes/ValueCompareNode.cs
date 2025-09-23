using MissQ;

namespace NS
{
    public enum EValueCompareType
    {
        EqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        NotEqualTo,
        LessThan,
        LessThanOrEqualTo,
    }
    
    [Node("ValueCompare", "Common/Value/ValueCompare", ENodeFunctionType.Value, typeof(ValueCompareNodeRunner), CommonNodeCategory.Value)]
    public sealed class ValueCompareNode : Node
    {
        [Exposed]
        public EValueCompareType Op;
        
        [Port(EPortDirection.Input, typeof(FP), "A")]
        public string InPortA;
        
        [Port(EPortDirection.Input, typeof(FP), "B")]
        public string InPortB;
        
        [Port(EPortDirection.Output, typeof(bool), "Res")]
        public string OutPortVal;
    }

    public sealed class ValueCompareNodeRunner : NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (ValueCompareNode)node;
            var a = graphRunner.GetInPortVal<FP>(n.InPortA);
            var b = graphRunner.GetInPortVal<FP>(n.InPortB);
            var res = false;

            switch (n.Op)
            {
                case EValueCompareType.EqualTo:
                    res = a == b;
                    break;
                case EValueCompareType.GreaterThan:
                    res = a > b;
                    break;
                case EValueCompareType.GreaterThanOrEqualTo:
                    res = a >= b;
                    break;
                case EValueCompareType.NotEqualTo:
                    res = a != b;
                    break;
                case EValueCompareType.LessThan:
                    res = a < b;
                    break;
                case EValueCompareType.LessThanOrEqualTo:
                    res = a <= b;
                    break;
            }
            graphRunner.SetOutPortVal(n.OutPortVal, res);
        }
    }

}