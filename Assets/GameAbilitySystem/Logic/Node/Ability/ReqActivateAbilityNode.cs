using GAS.Logic.Value;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    
    [Node("ReqActivateAbility", "Ability/Exec/ReqActivateAbility", ENodeFunctionType.Flow, typeof(ReqActivateAbilityNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.Ability, "Request to activate ability")]
    public sealed class ReqActivateAbilityNode:Node
    {
        [Header("队列类型")]
        [ExposedProp]
        public EActivationQueueType QueueType;
        
        [Header("前摇时间")]
        [ExposedProp]
        [SerializeReference]
        public ValuePickerBase PreCastTime;
        
        [Header("施放时间")]
        [ExposedProp]
        [SerializeReference]
        public ValuePickerBase CastTime;
        
        [Header("后摇时间")]
        [ExposedProp]
        [SerializeReference]
        public ValuePickerBase PostCastTime;
        
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        
        [Port(EPortDirection.Input, typeof(GameEventArg), "EventArg")]
        public string InPortVal;
        
    }

    public sealed class ReqActivateAbilityNodeRunner : FlowNodeRunner
    {
        private ReqActivateAbilityNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ReqActivateAbilityNode)nodeAsset;
        }
        
        public override void Execute()
        {
            ExecuteDependentValNodes(_node.Id);
            
            if (GraphRunner.Context is GameAbilityGraphRunnerContext context)
            {
                var job = context.Ability.System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr
                    .Get<AbilityActivationReqJob>();

                var lv = (uint)context.Ability.Owner.GetSimpleAttributeVal(ESimpleAttributeType.Level);
                job.InitJob(new AbilityActivationReq()
                {
                    Ability = context.Ability,
                    CastCfg = new AbilityCastCfg()
                    {
                        PreCastTime = ValuePickerUtility.GetValue(_node.PreCastTime, context.Ability.Owner, lv),
                        CastTime = ValuePickerUtility.GetValue(_node.CastTime, context.Ability.Owner, lv),
                        PostCastTime = ValuePickerUtility.GetValue(_node.PostCastTime, context.Ability.Owner, lv),
                    },
                    EventArgs =  GraphRunner.GetInPortVal<GameEventArg>(_node.InPortVal),
                    QueueType = _node.QueueType
                });
                
                context.Ability.AddActivationReqJob(job);
            }
            Complete();
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