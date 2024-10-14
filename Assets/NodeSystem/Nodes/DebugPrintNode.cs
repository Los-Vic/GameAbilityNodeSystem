namespace NodeSystem.Nodes
{
    [Node("DebugPrint","Debug/DebugPrint")]
    public class DebugPrintNode:NodeSystemNode
    {
        [ExposedProp]
        public string Log;
    }
}