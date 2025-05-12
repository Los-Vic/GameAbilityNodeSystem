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
        private ValueEquationNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ValueEquationNode)nodeAsset;
        }
        
        public override void Execute()
        {
            var a = GraphRunner.GetInPortVal<FP>(_node.InPortA);
            var b = GraphRunner.GetInPortVal<FP>(_node.InPortB);
            FP res = 0;

            switch (_node.Op)
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
            GraphRunner.SetOutPortVal(_node.OutPortVal, res);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }

}