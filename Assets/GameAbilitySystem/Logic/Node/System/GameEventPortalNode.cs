using MissQ;
using NS;
using UnityEditor.Experimental.GraphView;
using Node = NS.Node;

namespace GAS.Logic
{
    public class GameEventNodeParam:PortalParamBase
    {
        public GameUnit EventSrcUnit; //not null
        public GameAbility EventSrcAbility; //nullable
        public GameEffect EventSrcEffect;  //nullable
        public GameUnit EventTargetUnit;   //nullable
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;
    }
    
    [Node("SplitGameEventParam", "System/SplitGameEventParam", ENodeFunctionType.Value, typeof(SplitGameEventParamNodeRunner), (int)ECommonNodeCategory.Value, NodeScopeDefine.System)]
    public class SplitGameEventParamNode : Node
    {
        [Port(Direction.Input, typeof(GameEventNodeParam), "EventParam")]
        public string InPortVal;

        [Port(Direction.Output, typeof(GameUnit), "SrcUnit")]
        public string OutPortSrcUnit;
        [Port(Direction.Output, typeof(GameAbility), "SrcAbility")]
        public string OutPortSrcAbility;
        [Port(Direction.Output, typeof(GameEffect), "SrcEffect")]
        public string OutPortSrcEffect;
        [Port(Direction.Output, typeof(GameUnit), "TargetUnit")]
        public string OutPortTargetUnit;
        [Port(Direction.Output, typeof(FP), "ValParam1")]
        public string OutPortVal1;
        [Port(Direction.Output, typeof(FP), "ValParam2")]
        public string OutPortVal2;
        [Port(Direction.Output, typeof(FP), "ValParam3")]
        public string OutPortVal3;
        [Port(Direction.Output, typeof(string), "StringParam")]
        public string OutPortString;
    }

    public class SplitGameEventParamNodeRunner : NodeRunner
    {
        private SplitGameEventParamNode _node;
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (SplitGameEventParamNode)nodeAsset;
        }

        public override void Execute()
        {
            var inVal = GraphRunner.GetInPortVal<GameEventNodeParam>(_node.InPortVal);
            if(inVal == null)
                return;
            
            GraphRunner.SetOutPortVal(_node.OutPortSrcUnit, inVal.EventSrcUnit);
            GraphRunner.SetOutPortVal(_node.OutPortSrcAbility, inVal.EventSrcAbility);
            GraphRunner.SetOutPortVal(_node.OutPortSrcEffect, inVal.EventSrcEffect);
            GraphRunner.SetOutPortVal(_node.OutPortTargetUnit, inVal.EventTargetUnit);
            GraphRunner.SetOutPortVal(_node.OutPortVal1, inVal.EventValue1);
            GraphRunner.SetOutPortVal(_node.OutPortVal2, inVal.EventValue2);
            GraphRunner.SetOutPortVal(_node.OutPortVal3, inVal.EventValue3);
            GraphRunner.SetOutPortVal(_node.OutPortString, inVal.EventString);
        }
    }
    
    
    [Node("GameEventPortal", "System/GameEvent", ENodeFunctionType.Portal, typeof(GamePortalPortalNodeRunner), (int)ECommonNodeCategory.Portal, NodeScopeDefine.System)]
    public class GameEventPortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [PortalType]
        public EGamePortal NodePortal;
        
        [Port(Direction.Output, typeof(GameEventNodeParam), "EventParam")]
        public string OutParam1;
        
        public override string DisplayName()
        {
            return NodePortal.ToString();
        }
    }
    
    public class GamePortalPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private GameEventPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (GameEventPortalNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetPortalParam(PortalParamBase paramBase)
        {
            if (paramBase is not GameEventNodeParam param) 
                return;
            GraphRunner.SetOutPortVal(_node.OutParam1, param);
        }

        public override void Execute()
        {
            Complete();
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
        
    }
}