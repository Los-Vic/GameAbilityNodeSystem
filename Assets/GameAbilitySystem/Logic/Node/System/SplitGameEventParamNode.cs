using MissQ;
using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    
    [Node("SplitGameEventParam", "AbilitySystem/GameEvent/SplitGameEventParam", ENodeFunctionType.Value, typeof(SplitGameEventParamNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.AbilitySystem)]
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
        private SplitGameEventParamNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (SplitGameEventParamNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<GameEventArg>(_node.InPortVal);
            if(inVal == null)
                return;
            
            GraphRunner.SetOutPortVal(_node.OutPortEventType, inVal.EventType);
            GraphRunner.SetOutPortVal(_node.OutPortSrcUnit, inVal.EventSrcUnit);
            GraphRunner.SetOutPortVal(_node.OutPortSrcAbility, inVal.EventSrcAbility);
            GraphRunner.SetOutPortVal(_node.OutPortSrcEffect, inVal.EventSrcEffect);
            GraphRunner.SetOutPortVal(_node.OutPortTargetUnit, inVal.EventTargetUnit);
            GraphRunner.SetOutPortVal(_node.OutPortVal1, inVal.EventValue1);
            GraphRunner.SetOutPortVal(_node.OutPortVal2, inVal.EventValue2);
            GraphRunner.SetOutPortVal(_node.OutPortVal3, inVal.EventValue3);
            GraphRunner.SetOutPortVal(_node.OutPortString, inVal.EventString);
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
        }
    }
    
    
}