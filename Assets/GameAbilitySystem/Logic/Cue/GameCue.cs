using GCL;
using MissQ; 

namespace GAS.Logic
{
    public struct PlayAttributeValChangeCueContext
    {
        public Handler<GameUnit> UnitHandler;
        public ESimpleAttributeType AttributeType;
        public ECompositeAttributeType CompositeAttributeType;
        public FP OldVal;
        public FP NewVal;
    }
    
    public struct PlayAbilityFxCueContext
    {
        public string GameCueName;
        public Handler<GameUnit> UnitHandler;
        public Handler<GameAbility> AbilityHandler;
        public FP Param;
        public Handler<GameUnit> SubUnitHandler;
        public bool IsPersistent;
    }

    public struct StopAbilityFxCueContext
    {
        public string GameCueName;
        public Handler<GameUnit> UnitHandler;
        public Handler<GameAbility> AbilityHandler;
    }
    
    public struct PlayEffectFxCueContext
    {
        public string GameCueName;
        public Handler<GameUnit> UnitHandler;
        public Handler<GameEffect> EffectHandler;
        public bool IsPersistent;
    }

    public struct StopEffectFxCueContext
    {
        public string GameCueName;
        public Handler<GameUnit> UnitHandler;
        public Handler<GameEffect> EffectHandler;
    }

    public struct UnitCreateCueContext
    {
        public Handler<GameUnit> UnitInstanceID;
    }
    
    public struct UnitDestroyCueContext
    {
        public Handler<GameUnit> UnitInstanceID;
    }
    
}