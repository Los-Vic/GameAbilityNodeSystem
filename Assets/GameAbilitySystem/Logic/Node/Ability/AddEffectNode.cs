using System.Collections.Generic;
using Gameplay.Common;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddEffect", "AbilitySystem/Action/AddEffect", ENodeType.Action, typeof(AddEffectNodeRunner),
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
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (AddEffectNode)node;
            base.Execute(graphRunner, node);
            
            var unit = graphRunner.GetInPortVal<GameUnit>(n.InUnitPort);
            if (unit == null)
            {
                GameLogger.LogWarning("Add effect failed, unit is null.");
                graphRunner.Abort();
                return;
            }

            var modiferVal = graphRunner.GetInPortVal<FP>(n.InModifierValPort);
            var lifetimeVal = graphRunner.GetInPortVal<FP>(n.InLifetimeValPort);
            
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var param = new GameEffectCreateParam()
            {
                Instigator = context.Ability.Owner,
                EffectCfg = new GameEffectCfg()
                {
                    Name = n.EffectName,
                    AttributeType = n.AttributeType,
                    ModifierOp = n.ModifierType,
                    LifeWithInstigator = n.LifeWithInstigator,
                    DeadEvent = n.DeadEvent,
                    EventFilters = n.EventFilters,
                    ModifierVal = modiferVal,
                    LifetimeVal = lifetimeVal,
                    UseLifetimeVal = n.UseLifetimeVal,
                    RollbackPolicy = n.RollbackPolicy,
                    IsPersistent = n.IsPersistent,
                    CueName = n.CueName
                }
            };
            
            var effect = context.Ability.System.EffectInstanceSubsystem.CreateEffect(ref param);
            unit.AddEffect(effect);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (AddEffectNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutFlowPort);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}