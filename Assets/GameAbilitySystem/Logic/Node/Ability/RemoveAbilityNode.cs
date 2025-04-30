using GameplayCommonLibrary;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("RemoveAbility", "AbilitySystem/Action/RemoveAbility", ENodeFunctionType.Value, typeof(RemoveAbilityNodeNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class RemoveAbilityNode: Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        
        [Port(EPortDirection.Input, typeof(GameUnit), "Target")]
        public string InPortTarget;
        
        [Header("AbilityAsset")]
        [Exposed]
        public AbilityAsset AbilityAsset;
    }

    public sealed class RemoveAbilityNodeNodeRunner : FlowNodeRunner
    {
        private RemoveAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (RemoveAbilityNode)nodeAsset;
        }
        
        public override void Execute()
        {
            ExecuteDependentValNodes(NodeId);

            if (_node.AbilityAsset == null)
            {
                GameLogger.LogWarning("Remove ability failed, ability asset is null.");
                Abort();
                return;
            }
            
            var target = GraphRunner.GetInPortVal<GameUnit>(_node.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Remove ability failed, target is null.");
                Abort();
                return;
            }
            
            target.RemoveAbility(_node.AbilityAsset.id);
            
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