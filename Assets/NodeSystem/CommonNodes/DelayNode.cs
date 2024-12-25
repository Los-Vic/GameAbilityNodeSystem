namespace NS
{
    [Node("Delay", "Common/Task/Delay",ENodeFunctionType.Flow, typeof(DelayFlowNodeRunner), (int)ECommonNodeCategory.Task)]
    public class DelayNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Input, typeof(float), "Duration")]
        public string InPortFloat;
    }
    
    public class DelayFlowNodeRunner:FlowNodeRunner
    {
        private DelayNode _node;
        private float _delay;
        private float _elapsedTime;
        private const string DelayTaskName = "DelayNodeTask";
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DelayNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(_node.Id);
            _delay = GraphRunner.GetInPortVal<float>(_node.InPortFloat);
            NodeSystemLogger.Log($"input delay [{_delay}]");
            
            var task = TaskScheduler.CreateTask(DelayTaskName, GraphRunner, StartTask, CompleteTask, CancelTask, UpdateTask);
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
        
        private ETaskStatus UpdateTask(float dt)
        {
            _elapsedTime += dt;
            if (_elapsedTime >= _delay)
                return ETaskStatus.Completed;

            return ETaskStatus.Running;
        }

        private void CancelTask()
        {
        }

        private void CompleteTask()
        {
           Complete();
        }

        private ETaskStatus StartTask()
        {
            _elapsedTime = 0;
            return ETaskStatus.Running;
        }
    }
}