using GCL;
using MissQ;

namespace NS
{
    [Node("Delay", "Common/Task/Delay",ENodeFunctionType.Action, typeof(DelayFlowNodeRunner), CommonNodeCategory.Task)]
    public sealed class DelayNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Input, typeof(FP), "Duration")]
        public string InPortFP;
    }
    
    public sealed class DelayFlowNodeRunner:FlowNodeRunner
    {
        private FP _delay;
        private float _elapsedTime;
        private const string DelayTaskName = "DelayNodeTask";
        private NodeGraphRunner _graphRunner;

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            _graphRunner = graphRunner;
            var n = (DelayNode)node;
            _delay = graphRunner.GetInPortVal<FP>(n.InPortFP);
            GameLogger.Log($"Start delay [{_delay}], asset:{graphRunner.AssetName}, portal:{graphRunner.EntryName}");
            
            var task = graphRunner.TaskScheduler.CreateTask(DelayTaskName, graphRunner, OnStartTask, OnCompleteTask, OnCancelTask, OnUpdateTask);
            graphRunner.TaskScheduler.StartTask(task);
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var  n = (DelayNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
        
        private ETaskStatus OnUpdateTask(float dt)
        {
            _elapsedTime += dt;
            if (_elapsedTime >= _delay)
                return ETaskStatus.Completed;

            return ETaskStatus.Running;
        }

        private void OnCancelTask()
        {
        }

        private void OnCompleteTask()
        {
            _graphRunner.Forward();
        }

        private ETaskStatus OnStartTask()
        {
            _elapsedTime = 0;
            return ETaskStatus.Running;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _graphRunner = null;
        }
    }
}