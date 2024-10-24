﻿using NodeSystem.Ports;
using NodeSystem.Runners;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Print","Debug/Print", ENodeCategory.DebugFlowInstant, ENodeNumsLimit.None, typeof(DebugPrintNodeRunner))]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input, typeof(FlowPort))]
        public string InPort;
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;
        
    }
}