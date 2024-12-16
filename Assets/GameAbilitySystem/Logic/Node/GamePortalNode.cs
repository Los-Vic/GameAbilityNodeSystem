using System;
using NS;
using UnityEditor.Experimental.GraphView;
using Node = NS.Node;

namespace GAS.Logic
{
    [Serializable]
    public class NodeActionStartParam:PortalParamBase
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    [Node("GamePortal", "GameAbilitySystem/Portal/GamePortal", ENodeFunctionType.Portal, typeof(GamePortalPortalNodeRunner), (int)ECommonNodeCategory.Portal, NodeScopeDefine.Ability)]
    public class GamePortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [PortalType]
        public EGamePortal NodePortal;

        [Port(Direction.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(Direction.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return NodePortal.ToString();
        }
    }
    
    public class GamePortalPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private NodeGraphRunner _runner;
        private GamePortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (GamePortalNode)nodeAsset;
            _runner = graphRunner;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetPortalParam(PortalParamBase paramBase)
        {
            if (paramBase is not NodeActionStartParam param) 
                return;
            _runner.SetOutPortVal(_node.OutIntParam1, param.IntParam1);
            _runner.SetOutPortVal(_node.OutIntParam2, param.IntParam2);
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