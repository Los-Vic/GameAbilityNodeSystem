using System.Collections.Generic;
using GCL;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddEffect", "AbilitySystem/Action/AddEffect", ENodeFunctionType.Action, typeof(AddEffectNodeRunner),
        NodeCategoryDefine.EffectNode, NodeScopeDefine.AbilitySystem)]
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
        public bool IsPersistent;
        
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

        [Header("Cue")] 
        [Exposed]
        public string CueName;
    }
    
    public sealed class AddEffectNodeRunner : FlowNodeRunner
    {
        private AddEffectNode _node;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (AddEffectNode)context.Node;
        }

        public override void Execute()
        {
            base.Execute();
            
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
                    IsPersistent = _node.IsPersistent,
                    CueName = _node.CueName
                }
            };
            
            var effect = context.Ability.System.EffectInstanceSubsystem.CreateEffect(ref param);
            unit.AddEffect(effect);

            if (!_node.IsPersistent)
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