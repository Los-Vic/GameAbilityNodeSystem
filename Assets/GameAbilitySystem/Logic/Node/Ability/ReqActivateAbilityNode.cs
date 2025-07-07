using GameplayCommonLibrary;
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
        private ReqActivateAbilityNode _node;

        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ReqActivateAbilityNode)nodeAsset;
        }
        
        public override void Execute()
        {
            base.Execute();
            var context = (GameAbilityGraphRunnerContext)GraphRunner.Context;

            var job = context.Ability.Sys.ClassObjectPoolSubsystem.ClassObjectPoolMgr
                .Get<AbilityActivationReqJob>();

            if (!context.Ability.Sys.GetRscFromHandler(context.Ability.Owner, out var owner))
            {
                GameLogger.LogError($"Failed to get owner of {context.Ability}");
                Abort();
            }
            
            var preCast = ValuePickerUtility.GetValue(_node.PreCastTime, owner, context.Ability.Lv);
            var cast = ValuePickerUtility.GetValue(_node.CastTime, owner, context.Ability.Lv);
            var postCast = ValuePickerUtility.GetValue(_node.PostCastTime, owner, context.Ability.Lv);
            var clamp = ValuePickerUtility.GetValue(_node.CastProcessClampTime, owner,
                context.Ability.Lv);

            var total = preCast + cast + postCast;
            if (clamp > 0 && total > clamp)
            {
                var ratio = clamp / total;
                preCast *= ratio;
                cast *= ratio;
                postCast *= ratio;
            }

            job.InitJob(context.Ability.Sys, new AbilityActivationReq()
            {
                Ability = context.Ability.Handler,
                CastCfg = new AbilityCastCfg()
                {
                    PreCastTime = preCast,
                    CastTime = cast,
                    PostCastTime = postCast,
                },
                EventArgs = GraphRunner.GetInPortVal<GameEventArg>(_node.InPortVal)?.Handler ?? 0,
                QueueType = _node.QueueType
            });

            context.Ability.AddActivationReqJob(job);

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

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
}