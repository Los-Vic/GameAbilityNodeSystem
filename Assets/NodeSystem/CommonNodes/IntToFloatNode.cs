﻿using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("IntToFloat", "Common/LiteralValue/IntToFloat",ENodeFunctionType.Value ,  typeof(IntToFloatNodeRunner), (int)ECommonNodeCategory.Value )]
    public class IntToFloatNode:Node
    {
        [Port(Direction.Input, typeof(int))]
        public string InPortVal;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public class IntToFloatNodeRunner:NodeRunner
    {
        private IntToFloatNode _node;
        private NodeGraphRunner _graphRunner;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (IntToFloatNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute()
        {
            var inVal = _graphRunner.GetInPortVal<int>(_node.InPortVal);
            var floatVal = (float)inVal;
            _graphRunner.SetOutPortVal(_node.OutPortVal, floatVal);
        }
    }
}