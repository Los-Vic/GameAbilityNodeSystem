using NS;

namespace GAS.Logic.Command
{
    [Node("SpawnUnit", "AbilitySystem/Command/SpawnUnit", ENodeFunctionType.Value, typeof(CmdSpawnUnitNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public class CmdSpawnUnitNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Output, typeof(GameUnit), "NewUnit")]
        public string OutPortUnit;

        [Exposed]
        public string UnitName;
        [Exposed]
        public int PlayerIndex;
    }
    
     public sealed class CmdSpawnUnitNodeRunner : FlowNodeRunner
    {
        private CmdSpawnUnitNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (CmdSpawnUnitNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(NodeId);
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var unit = context.Ability.System.CommandDelegator.SpawnUnit(_node.UnitName, _node.PlayerIndex);
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