using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    [Node("Print","Default/Debug/Print", ENodeCategory.ExecDebugInstant, ENodeNumsLimit.None, typeof(DebugPrintNodeRunner))]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input, typeof(ExecutePort))]
        public string InPortExec;
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;
        
    }
    
    public class DebugPrintNodeRunner:NodeSystemFlowNodeRunner
    {
        private DebugPrintNode _node;
        private NodeSystemGraphRunner _graphRunner;
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (DebugPrintNode)nodeAsset;
            _graphRunner = graphRunner;
        }

        public override void Execute(float dt = 0)
        {
            Debug.Log(_node.Log);
            Complete();
        }

        public override string GetNextNode()
        {
            var port = _graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return default;
            var connectPort = _graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}