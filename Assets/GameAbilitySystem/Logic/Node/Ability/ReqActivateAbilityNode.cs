using GCL;
using GAS.Logic.Value;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    
    [Node("ReqActivateAbility", "AbilitySystem/Action/ReqActivateAbility", ENodeFunctionType.Action, typeof(ReqActivateAbilityNodeRunner), 
        CommonNodeCategory.Action, NodeScopeDefine.AbilitySystem, "Request to activate ability")]
    public sealed class ReqActivateAbilityNode:Node
    {
        [Header("队列类型")]
        [Exposed]
        public EActivationQueueType QueueType;
        
        [Header("前摇时间")]
        [Exposed]
        [SerializeReference]
        public ValuePickerBase PreCastTime;
        
        [Header("施放时间")]
        [Exposed]
        [SerializeReference]
        public ValuePickerBase CastTime;
        
        [Header("后摇时间")]
        [Exposed]
        [SerializeReference]
        public ValuePickerBase PostCastTime;

        [Header("完整前后摇过程的时间上限")]
        [Tooltip("如果过程超过上限，各步骤会等比例的缩短时长")]
        [Exposed]
        [SerializeReference]
        public ValuePickerBase CastProcessClampTime;
        
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InPortExec;
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        
        [Port(EPortDirection.Input, typeof(GameEventArg), "EventArg")]
        public string InPortVal;
        
    }

    public sealed class ReqActivateAbilityNodeRunner : FlowNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (ReqActivateAbilityNode)node;
            var context = (GameAbilityGraphRunnerContext)graphRunner.Context;

            var job = context.Ability.System.ClassObjectPoolSubsystem.Get<AbilityActivationReqJob>();

            if (!context.Ability.System.HandlerManagers.UnitHandlerMgr.DeRef(context.Ability.Owner, out var owner))
            {
                GameLogger.LogError($"Failed to get owner of {context.Ability}");
                graphRunner.Abort();
            }
            
            var preCast = ValuePickerUtility.GetValue(n.PreCastTime, owner, context.Ability.Lv);
            var cast = ValuePickerUtility.GetValue(n.CastTime, owner, context.Ability.Lv);
            var postCast = ValuePickerUtility.GetValue(n.PostCastTime, owner, context.Ability.Lv);
            var clamp = ValuePickerUtility.GetValue(n.CastProcessClampTime, owner,
                context.Ability.Lv);

            var total = preCast + cast + postCast;
            if (clamp > 0 && total > clamp)
            {
                var ratio = clamp / total;
                preCast *= ratio;
                cast *= ratio;
                postCast *= ratio;
            }

            job.InitJob(new AbilityActivationReq()
            {
                Ability = context.Ability.Handler,
                CastCfg = new AbilityCastCfg()
                {
                    PreCastTime = preCast,
                    CastTime = cast,
                    PostCastTime = postCast,
                },
                EventArgs = graphRunner.GetInPortVal<GameEventArg>(n.InPortVal)?.Handler ?? 0,
                QueueType = n.QueueType
            });

            context.Ability.AddActivationReqJob(job);
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n =  (ReqActivateAbilityNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}