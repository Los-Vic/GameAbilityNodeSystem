namespace NS
{
    public class NodeSystemFlowNodeRunner:NodeSystemNodeRunner
    {
        private bool _isCompleted;
        protected bool DependentValNodesExecuted { get; private set; }
        
        public virtual void Reset()
        {
            _isCompleted = false;
            DependentValNodesExecuted = false;
        }
        
        public virtual string GetNextNode()
        {
            return default;
        }

        public bool IsCompleted() => _isCompleted;
        protected void Complete() => _isCompleted = true;
        
        protected void ExecuteDependentValNodes(string flowNodeId, NodeSystemGraphRunner graphRunner)
        {
            if(DependentValNodesExecuted)
                return;
            
            var nodeList = graphRunner.GraphAssetRuntimeData.GetDependentNodeIds(flowNodeId);
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = graphRunner.NodeRunners[nodeList[i]];
                runner.Execute();
            }

            DependentValNodesExecuted = true;
        }
    }
}