using GameplayCommonLibrary;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddAbility", "AbilitySystem/Action/AddAbility", ENodeFunctionType.Value, typeof(EndAbilityNodeNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem)]
    public sealed class AddAbilityNode: Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Port(EPortDirection.Input, typeof(FP), "Level")]
        public string InPortLv;
        
        [Port(EPortDirection.Input, typeof(FP), "Signal1")]
        public string InPortSignal1;
        
        [Port(EPortDirection.Input, typeof(FP), "Signal2")]
        public string InPortSignal2;
        
        [Port(EPortDirection.Input, typeof(FP), "Signal3")]
        public string InPortSignal3;
        
        [Port(EPortDirection.Input, typeof(GameUnit), "Target")]
        public string InPortTarget;
        
        [Header("AbilityAsset")]
        [Exposed]
        public AbilityAsset AbilityAsset;
    }
    
    public sealed class AddAbilityNodeNodeRunner : FlowNodeRunner
    {
        private AddAbilityNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AddAbilityNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(NodeId);

            if (_node.AbilityAsset == null)
            {
                GameLogger.LogWarning("Add ability failed, ability asset is null.");
                Abort();
                return;
            }
            
            var target = GraphRunner.GetInPortVal<GameUnit>(_node.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Add ability failed, target is null.");
                Abort();
                return;
            }

            var lv = GraphRunner.GetInPortVal<FP>(_node.InPortLv);
            var signal1 = GraphRunner.GetInPortVal<FP>(_node.InPortSignal1);
            var signal2 = GraphRunner.GetInPortVal<FP>(_node.InPortSignal2);
            var signal3 = GraphRunner.GetInPortVal<FP>(_node.InPortSignal3);
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var abilityCreateParam = new AbilityCreateParam()
            {
                Id = _node.AbilityAsset.id,

            };
            //todo: AddAbility
            
            Complete();
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}