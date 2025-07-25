using NS;

namespace GAS.Logic.Command
{
    [Node("SpawnUnit", "AbilitySystem/Command/SpawnUnit", ENodeFunctionType.Action, typeof(CmdSpawnUnitNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public class CmdSpawnUnitNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;

        [Port(EPortDirection.Input, typeof(int), "PlayerIndex")]
        public string InPlayerIndex;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Output, typeof(GameUnit), "NewUnit")]
        public string OutPortUnit;

        [Exposed]
        public string UnitName;
    }
    
     public sealed class CmdSpawnUnitNodeRunner : FlowNodeRunner
    {
        private CmdSpawnUnitNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (CmdSpawnUnitNode)context.Node;
        }

        public override void Execute()
        {
            base.Execute();
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var playerIndex = GraphRunner.GetInPortVal<int>(_node.InPlayerIndex);
            var unit = context.Ability.System.CommandDelegator.SpawnUnit(_node.UnitName, playerIndex);
            GraphRunner.SetOutPortVal(_node.OutPortUnit, unit);
            
            Complete();
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
        
        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}