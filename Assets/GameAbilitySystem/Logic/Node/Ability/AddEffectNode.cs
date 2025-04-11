using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddEffect", "Ability/Action/AddEffect", ENodeFunctionType.Action, typeof(AddEffectNodeRunner),
        NodeCategoryDefine.EffectNode, NodeScopeDefine.Ability)]
    public sealed class AddEffectNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InFlowPort;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InUnitPort;
        
        [Port(EPortDirection.Input, typeof(FP), "ModifierVal")]
        public string InModifierValPort;
        
        [Port(EPortDirection.Input, typeof(FP), "LifetimeVal(op)")]
        public string InLifetimeValPort;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutFlowPort;
        
        [Header("Effect")]
        [Exposed]
        public string EffectName;
        [Exposed]
        public bool NotInstant;
        
        [Header("Modifier")]
        [Exposed]
        public ESimpleAttributeType AttributeType;
        [Exposed]
        public EModifierOp ModifierType;
        [Exposed]
        public EModifyRollbackPolicy RollbackPolicy;

        [Header("If not instant")] 
        [Exposed]
        public bool UseLifetimeVal;
        [Exposed]
        public bool LifeWithInstigator;
        [Exposed]
        public EGameEventType DeadEvent;
        [Exposed]
        public List<EGameEventFilter> EventFilters;
        
    }
    
    public sealed class AddEffectNodeRunner : FlowNodeRunner
    {
        private AddEffectNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AddEffectNode)nodeAsset;
        }

        public override void Execute()
        {
            ExecuteDependentValNodes(NodeId);
            
            var unit = GraphRunner.GetInPortVal<GameUnit>(_node.InUnitPort);
            if (unit == null)
            {
                GameLogger.LogWarning("Add effect failed, unit is null.");
                Abort();
                return;
            }

            var modiferVal = GraphRunner.GetInPortVal<FP>(_node.InModifierValPort);
            var lifetimeVal = GraphRunner.GetInPortVal<FP>(_node.InLifetimeValPort);
            
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;
            var param = new GameEffectCreateParam()
            {
                Instigator = context.Ability.Owner,
                EffectCfg = new GameEffectCfg()
                {
                    Name = _node.EffectName,
                    AttributeType = _node.AttributeType,
                    ModifierOp = _node.ModifierType,
                    LifeWithInstigator = _node.LifeWithInstigator,
                    DeadEvent = _node.DeadEvent,
                    EventFilters = _node.EventFilters,
                    ModifierVal = modiferVal,
                    LifetimeVal = lifetimeVal,
                    UseLifetimeVal = _node.UseLifetimeVal,
                    RollbackPolicy = _node.RollbackPolicy,
                    NotInstant = _node.NotInstant
                }
            };
            
            var effect = context.Ability.System.GetSubsystem<EffectInstanceSubsystem>().CreateEffect(ref param);
            unit.AddEffect(effect);

            if (!_node.NotInstant)
            {
                unit.RemoveEffect(effect);
            }
            
            Complete();
        }

        public override string GetNextNode()
        {
            var port = GraphRunner.GraphAssetRuntimeData.GetPortById(_node.OutFlowPort);
            if(!port.IsConnected())
                return null;
            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }

        public override void OnReturnToPool()
        {
            _node = null;
            base.OnReturnToPool();
        }
    }
}