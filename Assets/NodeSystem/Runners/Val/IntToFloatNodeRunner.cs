namespace NS
{
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