using GCL;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddTag", "AbilitySystem/Action/AddTag", ENodeType.Value, typeof(AddTagNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class AddTagNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Input, typeof(GameUnit), "Target")]
        public string InPortTarget;
        
        [Header("AddTag")] 
        [Exposed]
        public EGameTag Tag;
    }

    public sealed class AddTagNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (AddTagNode)node;
            base.Execute(graphRunner, node);
            
            var target = graphRunner.GetInPortVal<GameUnit>(n.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Add tag failed, target is null.");
                graphRunner.Abort();
                return;
            }
            
            target.AddTag(n.Tag);
            graphRunner.Forward();
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (AddTagNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}