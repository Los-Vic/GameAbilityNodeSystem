using System.Collections.Generic;
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
    
    public class AbilityActivationReqMgr
    {
        private readonly GameAbilitySystem _system;
        private readonly Queue<AbilityActivationReq> _worldQueue = new();
        private readonly Dictionary<int, Queue<AbilityActivationReq>> _playerQueues = new();
        private readonly Dictionary<GameUnit, Queue<AbilityActivationReq>> _unitQueues = new();

        public AbilityActivationReqMgr(GameAbilitySystem system)
        {
            _system = system;
        }
        
        
        
    }
}