using MissQ;
using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("GameEventParam", "System/GameEvent/GameEventParam", ENodeFunctionType.Value , typeof(GameEventParamNodeRunner), CommonNodeCategory.Value)]
    public class GameEventParamNode:Node
    {
        [Port(EPortDirection.Input, typeof(GameEventNodeParam), "EventParam")]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(EGameEventPortal), "EventType")]
        public string OutPortEventType;
        [Port(EPortDirection.Output, typeof(GameUnit), "SrcUnit")]
        public string OutPortSrcUnit;
        [Port(EPortDirection.Output, typeof(GameAbility), "SrcAbility")]
        public string OutPortSrcAbility;
        [Port(EPortDirection.Output, typeof(GameEffect), "SrcEffect")]
        public string OutPortSrcEffect;
        [Port(EPortDirection.Output, typeof(GameUnit), "TargetUnit")]
        public string OutPortTargetUnit;
        [Port(EPortDirection.Output, typeof(FP), "ValParam1")]
        public string OutPortVal1;
        [Port(EPortDirection.Output, typeof(FP), "ValParam2")]
        public string OutPortVal2;
        [Port(EPortDirection.Output, typeof(FP), "ValParam3")]
        public string OutPortVal3;
        [Port(EPortDirection.Output, typeof(string), "StringParam")]
        public string OutPortString;
    }

    public class GameEventParamNodeRunner : NodeRunner
    {
        
    }
}