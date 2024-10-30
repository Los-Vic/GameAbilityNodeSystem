namespace NS
{
    public class IntNodeRunner:NodeSystemNodeRunner
    {
        private IntNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = ((IntNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}