using System;

namespace NS
{
    [Node("RoundToInt", "Common/LiteralValue/RoundToInt",ENodeFunctionType.Value ,  typeof(IntToFloatNodeRunner), CommonNodeCategory.Value )]
    public class RoundToIntNode:Node
    {
        [Port(EPortDirection.Input, typeof(float))]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(int))]
        public string OutPortVal;
    }
    
    public class RoundToIntNodeRunner:NodeRunner
    {
        private RoundToIntNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (RoundToIntNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<int>(_node.InPortVal);
            var floatVal = (float)inVal;
            var intVal = Math.Round(floatVal);
            GraphRunner.SetOutPortVal(_node.OutPortVal, intVal);
        }
    }
}