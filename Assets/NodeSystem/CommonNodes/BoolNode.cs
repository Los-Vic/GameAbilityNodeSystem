using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Bool", "Common/LiteralValue/Bool", ENodeFunctionType.Value ,typeof(BoolNodeRunner), (int)ECommonNodeCategory.Value)]
    public class BoolNode : Node
    {
        [ExposedProp] public bool Val;

        [Port(Direction.Output, typeof(bool))] 
        public string OutPortVal;
    }
    
    public class BoolNodeRunner:NodeRunner
    {
        private BoolNode _node;
        private NodeGraphRunner _graphRunner;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = ((BoolNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute()
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}