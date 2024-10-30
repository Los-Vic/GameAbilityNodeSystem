namespace NS
{
    public class BoolNodeRunner:NodeSystemNodeRunner
    {
        private BoolNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = ((BoolNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}