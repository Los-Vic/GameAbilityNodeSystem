using NS;

namespace GAS.Logic.Command
{
    [Node("SpawnUnit", "AbilitySystem/Command/SpawnUnit", ENodeType.Action, typeof(CmdSpawnUnitNodeRunner), 
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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (CmdSpawnUnitNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var playerIndex = graphRunner.GetInPortVal<int>(n.InPlayerIndex);
            var unit = context.Ability.System.CommandDelegator.SpawnUnit(n.UnitName, playerIndex);
            graphRunner.SetOutPortVal(n.OutPortUnit, unit);
            graphRunner.Forward();
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (CmdSpawnUnitNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}