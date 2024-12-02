namespace NS
{
    public class NodeSystemFlowNodeRunner:NodeSystemNodeRunner
    {
        private bool _dependentValNodesExecuted;
        protected NodeSystemGraphRunner GraphRunner { get; private set; }

        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            GraphRunner = graphRunner;
        }

        public virtual void Reset()
        {
            _dependentValNodesExecuted = false;
        }
        
        public virtual string GetNextNode()
        {
            return default;
        }

        protected void Complete()
        {
            GraphRunner.MoveToNextNode();
            GraphRunner.ExecuteRunner();
        }
        
        protected void ExecuteDependentValNodes(string flowNodeId)
        {
            if(_dependentValNodesExecuted)
                return;
            
            var nodeList = GraphRunner.GraphAssetRuntimeData.GetDependentNodeIds(flowNodeId);
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = GraphRunner.GetNodeRunner(nodeList[i]);
                runner.Execute();
            }

            _dependentValNodesExecuted = true;
        }

        public override void OnReturnToPool()
        {
            Reset();
        }
    }
}