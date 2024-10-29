﻿using NodeSystem.Core;
using NodeSystem.Ports;
using NodeSystem.Runners;
using UnityEditor.Experimental.GraphView;

namespace NodeSystem.Nodes
{
    [Node("Delay", "Executable/Delay", ENodeCategory.ExecNonInstant, ENodeNumsLimit.None, typeof(DelayNodeRunner))]
    public class DelayNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(FlowPort))]
        public string InPort;
        [Port(Direction.Output, typeof(FlowPort))]
        public string OutPort;

        [Port(Direction.Input, typeof(float), "delay")]
        public string InFloatPort;
    }
}