using GCL;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("RemoveTag", "AbilitySystem/Action/RemoveTag", ENodeFunctionType.Value, typeof(RemoveTagNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class RemoveTagNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Input, typeof(GameUnit), "Target")]
        public string InPortTarget;
        
        [Header("RemoveTag")] 
        [Exposed]
        public EGameTag Tag;
    }

    public sealed class RemoveTagNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (RemoveTagNode)node;
            var target = graphRunner.GetInPortVal<GameUnit>(n.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Remove tag failed, target is null.");
                graphRunner.Abort();
                return;
            }
            
            target.RemoveTag(n.Tag);
            graphRunner.Forward();
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (RemoveTagNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}