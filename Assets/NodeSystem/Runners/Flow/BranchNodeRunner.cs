namespace NS
{
    public class BranchNodeRunner:NodeSystemFlowNodeRunner
    {
        private BranchNode _node;
        private NodeSystemGraphRunner _graphRunner;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
           _node = (BranchNode)nodeAsset;
           graphRunner = _graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            if (!DependentValNodesExecuted)
            {
                ExecuteDependentValNodes(_node.Id, _graphRunner);
                
            }
        }
    }
}