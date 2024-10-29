﻿using NodeSystem.Core;
using NodeSystem.Ports;
using NodeSystem.Runners;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Start", "Flow/Start", ENodeCategory.Start, ENodeNumsLimit.Singleton, typeof(StartNodeRunner))]
    public class StartNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;
    }
}