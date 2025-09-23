using GCL;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("RemoveAbility", "AbilitySystem/Action/RemoveAbility", ENodeFunctionType.Value, typeof(RemoveAbilityNodeRunner), 
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

    public sealed class RemoveAbilityNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (RemoveAbilityNode)node;
            if (!n.AbilityAsset)
            {
                GameLogger.LogWarning("Remove ability failed, ability asset is null.");
                graphRunner.Abort();
                return;
            }
            
            var target = graphRunner.GetInPortVal<GameUnit>(n.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Remove ability failed, target is null.");
                graphRunner.Abort();
                return;
            }
            
            target.RemoveAbility(n.AbilityAsset.id);
            
            graphRunner.Forward();
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (RemoveAbilityNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}