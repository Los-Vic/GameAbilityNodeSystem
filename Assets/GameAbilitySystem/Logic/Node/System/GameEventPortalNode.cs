using MissQ;
using NS;
using UnityEngine.Serialization;
using Node = NS.Node;

namespace GAS.Logic
{
    public class GameEventNodeParam:PortalParamBase
    {
        public EGameEventPortal EventType;
        public GameUnit EventSrcUnit; //not null
        public GameAbility EventSrcAbility; //nullable
        public GameEffect EventSrcEffect;  //nullable
        public GameUnit EventTargetUnit;   //nullable
        public FP EventValue1;
        public FP EventValue2;
        public FP EventValue3;
        public string EventString;
    }
    
    [Node("GameEventPortal", "System/GameEvent/GameEventPortal", ENodeFunctionType.Portal, typeof(GamePortalPortalNodeRunner), CommonNodeCategory.Portal, NodeScopeDefine.System)]
    public class GameEventPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [FormerlySerializedAs("NodePortal")] [PortalType]
        public EGameEventPortal nodeEventPortal;
        
        [Port(EPortDirection.Output, typeof(GameEventNodeParam), "EventParam")]
        public string OutParam1;
        
        public override string DisplayName()
        {
            return nodeEventPortal.ToString();
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