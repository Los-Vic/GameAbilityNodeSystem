using UnityEditor.Experimental.GraphView;

namespace NS
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