using MissQ;

namespace GAS.Logic
{
    public struct PlayAttributeValChangeCueContext
    {
        public int UnitInstanceID;
        public ESimpleAttributeType AttributeType;
        public ECompositeAttributeType CompositeAttributeType;
        public FP OldVal;
        public FP NewVal;
    }
    
    public struct PlayAbilityFxCueContext
    {
        public string GameCueName;
        public int UnitInstanceID;
        public int AbilityInstanceID;
        public FP Param;
        public int SubUnitInstanceID;
    }

    public struct StopAbilityFxCueContext
    {
        public string GameCueName;
        public int UnitInstanceID;
        public int AbilityInstanceID;
    }

    public struct UnitCreateCueContext
    {
        public int UnitInstanceID;
    }
    
    public struct UnitDestroyCueContext
    {
        public int UnitInstanceID;
    }
}