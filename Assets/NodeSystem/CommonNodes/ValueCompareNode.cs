using MissQ;
using UnityEngine;

namespace NS
{
    public enum EValueCompareType
    {
        [InspectorName("等于")]
        EqualTo,
        [InspectorName("大于")]
        GreaterThan,
        [InspectorName("大等于")]
        GreaterThanOrEqualTo,
    }
    
    [Node("ValueCompare", "Common/Value/ValueCompare", ENodeFunctionType.Value, typeof(ValueCompareNodeRunner), CommonNodeCategory.Value)]
    public sealed class ValueCompareNode : Node
    {
        [ExposedProp]
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
        private ValueCompareNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ValueCompareNode)nodeAsset;
        }
        
        public override void Execute()
        {
            var a = GraphRunner.GetInPortVal<FP>(_node.InPortA);
            var b = GraphRunner.GetInPortVal<FP>(_node.InPortB);
            var res = false;

            switch (_node.Op)
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