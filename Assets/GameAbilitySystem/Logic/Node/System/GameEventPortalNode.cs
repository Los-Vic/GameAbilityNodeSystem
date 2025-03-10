﻿using NS;
using UnityEngine.Serialization;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("GameEventPortal", "System/GameEvent/GameEventPortal", ENodeFunctionType.Portal, typeof(GamePortalPortalNodeRunner), CommonNodeCategory.Portal, NodeScopeDefine.System)]
    public sealed class GameEventPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [FormerlySerializedAs("nodeEventPortal")] [FormerlySerializedAs("NodePortal")] [PortalType]
        public EGameEventType nodeEventType;
        
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventParam")]
        public string OutParam1;
        
        public override string DisplayName()
        {
            return nodeEventType.ToString();
        }
    }
    
    public sealed class GamePortalPortalNodeRunner:PortalNodeRunner
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

        public override void SetPortalParam(IPortalParam paramBase)
        {
            if (paramBase is not GameEventArg param) 
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

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _node = null;
            _nextNode = null;
        }
    }
}