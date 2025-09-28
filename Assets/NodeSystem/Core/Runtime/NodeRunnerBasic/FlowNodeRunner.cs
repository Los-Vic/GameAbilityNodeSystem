namespace NS
{
    public class FlowNodeRunner:NodeRunner
    {
        private bool _dependentValNodesExecuted;
        public virtual string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            return null;
        }
        
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            ExecuteDependentValNodes(graphRunner, node);
        }

        private void ExecuteDependentValNodes(NodeGraphRunner graphRunner, Node node)
        {
            if(_dependentValNodesExecuted)
                return;
            
            var nodeList = graphRunner.GraphAssetRuntimeData.GetDependentNodeIds(node.Id);
            for (var i = nodeList.Count - 1; i >= 0; i--)
            {
                var runner = graphRunner.CreateNodeRunner(nodeList[i]);
                runner.Execute(graphRunner, node);
                graphRunner.DestroyNodeRunner(runner);
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