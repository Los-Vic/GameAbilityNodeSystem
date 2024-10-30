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
            var inValObj = _graphRunner.GetInPortVal(_node.InPortVal);
            var inVal = inValObj != null ? (int)inValObj : 0;
            var floatVal = (float)inVal;
            _graphRunner.SetOutPortVal(_node.OutPortVal, floatVal);
        }
    }
}