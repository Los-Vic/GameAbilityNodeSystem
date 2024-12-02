using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Bool", "Default/Literal/Bool", ENodeCategory.Value, ENodeNumsLimit.None, typeof(BoolNodeRunner))]
    public class BoolNode : NodeSystemNode
    {
        [ExposedProp] public bool Val;

        [Port(Direction.Output, typeof(bool))] 
        public string OutPortVal;
    }
    
    public class BoolNodeRunner:NodeSystemNodeRunner
    {
        private BoolNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
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