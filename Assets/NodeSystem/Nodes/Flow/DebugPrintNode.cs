using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Print","Debug/Print", ENodeCategory.ExecDebugInstant, ENodeNumsLimit.None, typeof(DebugPrintNodeRunner))]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;

        [Port(Direction.Input, typeof(ExecutePort))]
        public string InPortExec;
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;
        
    }
}