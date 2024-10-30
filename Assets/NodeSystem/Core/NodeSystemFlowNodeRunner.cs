namespace NS
{
    public class NodeSystemFlowNodeRunner:NodeSystemNodeRunner
    {
        public bool IsNodeRunnerCompleted { get; protected set; }
        protected bool DependentValNodesExecuted { get; private set; }
        
        public virtual void Reset()
        {
            IsNodeRunnerCompleted = false;
            DependentValNodesExecuted = false;
        }
        
        public virtual string GetNextNode()
        {
            return default;
        }

        protected void ExecuteDependentValNodes(string flowNodeId, NodeSystemGraphRunner graphRunner)
        {
            if(DependentValNodesExecuted)
                return;
            
            var nodeList = graphRunner.GraphAssetRuntimeData.NodeValDependencyMap[flowNodeId];
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = graphRunner.NodeRunners[nodeList[i]];
                runner.Execute();
            }

            DependentValNodesExecuted = true;
        }
    }
}