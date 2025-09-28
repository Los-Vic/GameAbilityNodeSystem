using GCL;

namespace GAS.Logic
{
    public class HandlerMgrCollection
    {
        public readonly HandlerMgr<GameUnit> UnitHandlerMgr = new();
        public readonly HandlerMgr<GameAbility> AbilityHandlerMgr = new();
        public readonly HandlerMgr<GameEffect> EffectHandlerMgr = new();
        public readonly HandlerMgr<GameEventArg> EventArgHandlerMgr = new();
    }
}