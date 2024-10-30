using UnityEngine;

namespace NS
{
    public class FloatNodeRunner:NodeSystemNodeRunner
    {
        private FloatNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((FloatNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log($"Execute Float Node, Float Val[{_node.Val}]");
            _graphRunner.OutPortResultCached.Add(_node.OutVal, _node.Val);
        }
    }
}