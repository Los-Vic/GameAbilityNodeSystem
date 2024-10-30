using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("Branch", "Flow/Branch", ENodeCategory.FlowControl, ENodeNumsLimit.None, typeof(BranchNodeRunner))]
    public class BranchNode:NodeSystemNode
    {
        [Port(Direction.Input,typeof(ExecutePort))]
        public string InPortExec;

        [Port(Direction.Input,typeof(bool), "Condition")]
        public string InPortBool;

        [Port(Direction.Output, typeof(ExecutePort), "True")]
        public string OutPortTrueExec;
        
        [Port(Direction.Output, typeof(ExecutePort), "False")]
        public string OutPortFalseExec;
    }
}