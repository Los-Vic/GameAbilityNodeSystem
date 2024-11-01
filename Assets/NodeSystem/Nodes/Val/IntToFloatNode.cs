using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("IntToFloat", "Default/Converter/IntToFloat", ENodeCategory.Value, ENodeNumsLimit.None, typeof(IntToFloatNodeRunner) )]
    public class IntToFloatNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(int))]
        public string InPortVal;

        [Port(Direction.Output, typeof(float))]
        public string OutPortVal;
    }
    
    public class IntToFloatNodeRunner:NodeSystemNodeRunner
    {
        private IntToFloatNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (IntToFloatNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            var inVal = _graphRunner.GetInPortVal<int>(_node.InPortVal);
            var floatVal = (float)inVal;
            _graphRunner.SetOutPortVal(_node.OutPortVal, floatVal);
        }
    }
}