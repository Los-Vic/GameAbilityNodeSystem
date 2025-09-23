using System;
namespace NS
{
    public enum ENodeDemoEntryType
    {
        BeginPlay,
        EndPlay
    }
    
    [Serializable]
    public class NodeDemoEntryParam:IEntryParam
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    [Node("DemoEntryEvent", "Demo/Entry/DemoEntryEvent", ENodeFunctionType.Entry, typeof(EntryEntryNodeRunner), CommonNodeCategory.Entry, -1)]
    public class DemoPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Entry]
        public ENodeDemoEntryType NodeDemoEntry;

        [Port(EPortDirection.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(EPortDirection.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string ToString()
        {
            return NodeDemoEntry.ToString();
        }
    }
    
    public class EntryEntryNodeRunner:EntryNodeRunner
    {
        public override void SetEntryParam(NodeGraphRunner graphRunner, Node node, IEntryParam entryParam)
        {
            if (entryParam is not NodeDemoEntryParam param) 
                return;
            var n = (DemoPortalNode)node;
            graphRunner.SetOutPortVal(n.OutIntParam1, param.IntParam1);
            graphRunner.SetOutPortVal(n.OutIntParam2, param.IntParam2);
        }

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var n = (DemoPortalNode)node;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
        
    }
}