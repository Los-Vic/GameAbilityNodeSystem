using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Delay", "Executable/Delay", ENodeCategory.ExecNonInstant, ENodeNumsLimit.None, typeof(DelayNodeRunner))]
    public class DelayNode:NodeSystemNode
    {
        [Port(Direction.Input, typeof(ExecutePort))]
        public string InPortExec;
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;

        [Port(Direction.Input, typeof(float), "Duration")]
        public string InPortFloat;
    }
}