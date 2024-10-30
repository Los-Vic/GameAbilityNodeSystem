namespace NS
{
    public class FloatNodeRunner:NodeSystemNodeRunner
    {
        private FloatNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = ((FloatNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            _graphRunner.SetOutPortVal(_node.OutPortVal, _node.Val);
        }
    }
}