using GameplayCommonLibrary;
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
        private RemoveTagNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (RemoveTagNode)context.Node;
        }
        
        public override void Execute()
        {
            base.Execute();
            
            var target = GraphRunner.GetInPortVal<GameUnit>(_node.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Remove tag failed, target is null.");
                Abort();
                return;
            }
            
            target.RemoveTag(_node.Tag);
            
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