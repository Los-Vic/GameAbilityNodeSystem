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
        private DelayNode _node;
        private FP _delay;
        private float _elapsedTime;
        private const string DelayTaskName = "DelayNodeTask";

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (DelayNode)context.Node;
        }

        public override void Execute()
        {
            base.Execute();
            _delay = GraphRunner.GetInPortVal<FP>(_node.InPortFP);
            GameLogger.Log($"Start delay [{_delay}], asset:{GraphRunner.AssetName}, portal:{GraphRunner.EntryName}");
            
            var task = TaskScheduler.CreateTask(DelayTaskName, GraphRunner, OnStartTask, OnCompleteTask, OnCancelTask, OnUpdateTask);
            TaskScheduler.StartTask(task);
        }
        
        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return default;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
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
           Complete();
        }

        private ETaskStatus OnStartTask()
        {
            _elapsedTime = 0;
            return ETaskStatus.Running;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}