namespace NS
{
    public class NodeSystemFlowNodeRunner:NodeSystemNodeRunner
    {
        private bool _isCompleted;
        private bool _dependentValNodesExecuted;
        
        public virtual void Reset()
        {
            _isCompleted = false;
            _dependentValNodesExecuted = false;
        }
        
        public virtual string GetNextNode()
        {
            return default;
        }

        public bool IsCompleted() => _isCompleted;
        protected void Complete() => _isCompleted = true;
        
        protected void ExecuteDependentValNodes(string flowNodeId, NodeSystemGraphRunner graphRunner)
        {
            if(_dependentValNodesExecuted)
                return;
            
            var nodeList = graphRunner.GraphAssetRuntimeData.GetDependentNodeIds(flowNodeId);
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = graphRunner.GetNodeRunner(nodeList[i]);
                runner.Execute();
            }

            _dependentValNodesExecuted = true;
        }
    }
}