namespace NS
{
    public class FlowNodeRunner:NodeRunner
    {
        private bool _dependentValNodesExecuted;
        protected INodeSystemTaskScheduler TaskScheduler => GraphRunner.TaskScheduler;
        
        public virtual string GetNextNode()
        {
            return null;
        }

        protected void Complete()
        {
            GraphRunner.ForwardRunner();
        }

        protected void Abort()
        {
            GraphRunner.AbortRunner();
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(NodeId);
        }

        private void ExecuteDependentValNodes(string flowNodeId)
        {
            if(_dependentValNodesExecuted)
                return;
            
            var nodeList = GraphRunner.GraphAssetRuntimeData.GetDependentNodeIds(flowNodeId);
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = GraphRunner.CreateNodeRunner(nodeList[i]);
                runner.Execute();
                GraphRunner.DestroyNodeRunner(runner);
            }

            _dependentValNodesExecuted = true;
        }

        public override void OnReturnToPool()
        {
            _dependentValNodesExecuted = false;
            base.OnReturnToPool();
        }
    }
}