using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public enum EActivationQueueType
    {
        None,
        Unit,
        Player,
        World
    }

    public struct AbilityCastCfg
    {
        public FP PreCastTime;
        public FP CastTime;
        public FP PostCastTime;
    }
    
    public struct AbilityActivationReqCfg
    {
        public EActivationQueueType QueueType;
        public AbilityCastCfg CastCfg;
    }
    
    public struct AbilityActivationReq
    {
        public GameAbility Ability;
        public AbilityActivationReqCfg Cfg;
    }


    public class AbilityActivationReqJob:IRefCountRequester
    {
        public AbilityActivationReq Req;
        
        public bool IsRequesterStillValid()
        {
            throw new System.NotImplementedException();
        }

        public string GetRequesterDesc()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class AbilityActivationReqSubsystem:GameAbilitySubsystem
    {
        private readonly Queue<AbilityActivationReqJob> _worldQueue = new();
        private readonly Dictionary<int, Queue<AbilityActivationReqJob>> _playerQueues = new();
        private readonly Dictionary<GameUnit, Queue<AbilityActivationReqJob>> _unitQueues = new();

        public override void Reset()
        {
            base.Reset();
        }

        public override void UnInit()
        {
            base.UnInit();
        }
        
        
    }
    
    
}