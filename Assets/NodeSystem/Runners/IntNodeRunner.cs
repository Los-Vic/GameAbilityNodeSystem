using NodeSystem.Nodes;
using UnityEngine;

namespace NodeSystem.Runners
{
    public class IntNodeRunner:NodeSystemNodeRunner
    {
        private IntNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = ((IntNode)nodeAsset);
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log($"Execute Int Node, Int Val[{_node.Val}]");
            IsNodeRunnerCompleted = true;
            _graphRunner.OutPortResultCached.Add(_node.OutVal, _node.Val);
        }
    }
}