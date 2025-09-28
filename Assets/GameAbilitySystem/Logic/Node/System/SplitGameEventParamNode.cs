using MissQ;
using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    
    [Node("SplitGameEventParam", "AbilitySystem/GameEvent/SplitGameEventParam", ENodeType.Value, typeof(SplitGameEventParamNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
    public sealed class SplitGameEventParamNode : Node
    {
        [Port(EPortDirection.Input, typeof(GameEventArg), "EventParam")]
        public string InPortVal;

        [Port(EPortDirection.Output, typeof(EGameEventType), "EventType")]
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

    public sealed class SplitGameEventParamNodeRunner : NodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            var n = (SplitGameEventParamNode)node;
            var inVal = graphRunner.GetInPortVal<GameEventArg>(n.InPortVal);
            if(inVal == null)
                return;
            
            graphRunner.SetOutPortVal(n.OutPortEventType, inVal.EventType);
            graphRunner.SetOutPortVal(n.OutPortSrcUnit, inVal.EventSrcUnit);
            graphRunner.SetOutPortVal(n.OutPortSrcAbility, inVal.EventSrcAbility);
            graphRunner.SetOutPortVal(n.OutPortSrcEffect, inVal.EventSrcEffect);
            graphRunner.SetOutPortVal(n.OutPortTargetUnit, inVal.EventTargetUnit);
            graphRunner.SetOutPortVal(n.OutPortVal1, inVal.EventValue1);
            graphRunner.SetOutPortVal(n.OutPortVal2, inVal.EventValue2);
            graphRunner.SetOutPortVal(n.OutPortVal3, inVal.EventValue3);
            graphRunner.SetOutPortVal(n.OutPortString, inVal.EventString);
        }
    }
    
    
}