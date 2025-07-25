using GameplayCommonLibrary;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddTag", "AbilitySystem/Action/AddTag", ENodeFunctionType.Value, typeof(AddTagNodeRunner), 
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
        private AddTagNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (AddTagNode)context.Node;
        }
        public override void Execute()
        {
            base.Execute();
            
            var target = GraphRunner.GetInPortVal<GameUnit>(_node.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Add tag failed, target is null.");
                Abort();
                return;
            }
            
            target.AddTag(_node.Tag);
            
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