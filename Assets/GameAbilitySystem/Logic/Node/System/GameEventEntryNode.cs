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
        
        public override string DisplayName()
        {
            return nodeEventType.ToString();
        }
    }
    
    public sealed class GameEventEntryNodeRunner:EntryNodeRunner
    {
        private string _nextNode;
        private GameEventEntryNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (GameEventEntryNode)context.Node;
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetEntryParam(IEntryParam paramBase)
        {
            if (paramBase is not GameEventArg param) 
                return;
            GraphRunner.SetOutPortVal(_node.OutParam1, param);
        }

        public override void Execute()
        {
            Complete();
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
            _nextNode = null;
        }
    }
}