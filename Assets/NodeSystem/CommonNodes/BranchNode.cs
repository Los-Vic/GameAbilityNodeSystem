﻿namespace NS
{
    [Node("Branch", "Common/FlowControl/Branch", ENodeFunctionType.Action, typeof(BranchFlowNodeRunner), CommonNodeCategory.FlowControl)]
    public sealed class BranchNode:Node
    {
        [Port(EPortDirection.Input,typeof(BaseFlowPort))]
        public string InPortExec;

        [Port(EPortDirection.Input,typeof(bool), "Condition")]
        public string InPortBool;

        [Port(EPortDirection.Output, typeof(BaseFlowPort), "True")]
        public string OutPortTrueExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "False")]
        public string OutPortFalseExec;
    }
    
    public sealed class BranchFlowNodeRunner:FlowNodeRunner
    {
        private BranchNode _node;
        private bool _condition;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (BranchNode)nodeAsset;
        }

        public override void Execute()
        {
            base.Execute();
            _condition = GraphRunner.GetInPortVal<bool>(_node.InPortBool);
            Complete();
        }

        public override string GetNextNode()
        {
            var outPortId =  _condition ? _node.OutPortTrueExec : _node.OutPortFalseExec;
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(outPortId);
            if (!port.IsConnected())
                return default;

            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}