using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Print","Debug/Print", ENodeCategory.ExecDebugInstant, ENodeNumsLimit.None, typeof(DebugPrintNodeRunner))]
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