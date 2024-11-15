﻿using System;
using GameAbilitySystem.Logic;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Serializable]
    public class NodeEventParam:NodeSystemEventParamBase
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    [Node("GameEvent", "GameAbilitySystem/Event/GameEvent", ENodeCategory.Event, ENodeNumsLimit.None, typeof(GameEventNodeRunner))]
    public class GameEventNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;

        [EventType]
        public EGameEvent NodeEvent;

        [Port(Direction.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(Direction.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return NodeEvent.ToString();
        }
    }
    
    public class GameEventNodeRunner:NodeSystemEventNodeRunner
    {
        private string _nextNode;
        private NodeSystemGraphRunner _runner;
        private GameEventNode _node;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
        {
            _node = (GameEventNode)nodeAsset;
            _runner = graphRunner;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetUpEventParam(NodeSystemEventParamBase paramBase)
        {
            if (paramBase is not NodeEventParam param) 
                return;
            _runner.SetOutPortVal(_node.OutIntParam1, param.IntParam1);
            _runner.SetOutPortVal(_node.OutIntParam2, param.IntParam2);
        }

        public override void Execute(float dt = 0)
        {
            Complete();
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
        
    }
}