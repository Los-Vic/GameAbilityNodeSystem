using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("GameEventEntry", "AbilitySystem/GameEvent/GameEventEntry", ENodeFunctionType.Entry, typeof(GameEventEntryNodeRunner), CommonNodeCategory.Entry, NodeScopeDefine.AbilitySystem)]
    public sealed class GameEventEntryNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Entry]
        public EGameEventType nodeEventType;
        
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventParam")]
        public string OutParam1;

        public override string ToString()
        {
            return nodeEventType.ToString();
        }
    }
    
    public sealed class GameEventEntryNodeRunner:EntryNodeRunner
    {
        public override void SetEntryParam(NodeGraphRunner graphRunner, Node node, IEntryParam paramBase)
        {
            if (paramBase is not GameEventArg param) 
                return;
            if(node is not GameEventEntryNode n)
                return;
            graphRunner.SetOutPortVal(n.OutParam1, param);
        }

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            if(node is not GameEventEntryNode n)
                return null;
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}