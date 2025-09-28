using GCL;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("AddAbility", "AbilitySystem/Action/AddAbility", ENodeType.Value, typeof(AddAbilityNodeRunner), 
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
    
    public sealed class AddAbilityNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not AddAbilityNode addAbilityNode)
            {
                graphRunner.Abort();
                return;
            }
            
            base.Execute(graphRunner, node);

            if (addAbilityNode.AbilityAsset == null)
            {
                GameLogger.LogWarning("Add ability failed, ability asset is null.");
                graphRunner.Abort();
                return;
            }
            
            var target = graphRunner.GetInPortVal<GameUnit>(addAbilityNode.InPortTarget);
            if (target == null)
            {
                GameLogger.LogWarning("Add ability failed, target is null.");
                graphRunner.Abort();
                return;
            }

            var lv = graphRunner.GetInPortVal<FP>(addAbilityNode.InPortLv);
            var signal1 = graphRunner.GetInPortVal<FP>(addAbilityNode.InPortSignal1);
            var signal2 = graphRunner.GetInPortVal<FP>(addAbilityNode.InPortSignal2);
            var signal3 = graphRunner.GetInPortVal<FP>(addAbilityNode.InPortSignal3);
            
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;
            var abilityCreateParam = new AbilityCreateParam()
            {
                Id = addAbilityNode.AbilityAsset.id,
                Lv = (uint)lv,
                SignalVal1 = signal1,
                SignalVal2 = signal2,
                SignalVal3 = signal3,
                Instigator = context.Ability.Owner
            };
            
            target.AddAbility(abilityCreateParam);
            graphRunner.Forward();
        }
        
        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not AddAbilityNode addAbilityNode)
            {
                return null;
            }
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(addAbilityNode.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}