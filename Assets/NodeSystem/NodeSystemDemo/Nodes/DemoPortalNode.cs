using System;
namespace NS
{
    public enum ENodeDemoPortalType
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
    
    [Node("DemoPortalEvent", "Demo/Portal/DemoPortalEvent", ENodeFunctionType.Entry, typeof(EntryEntryNodeRunner), CommonNodeCategory.Entry, -1)]
    public class DemoPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [Entry]
        public ENodeDemoPortalType NodeDemoPortal;

        [Port(EPortDirection.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(EPortDirection.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return NodeDemoPortal.ToString();
        }
    }
    
    public class EntryEntryNodeRunner:EntryNodeRunner
    {
        private string _nextNode;
        private DemoPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (DemoPortalNode)nodeAsset;
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetEntryParam(IEntryParam paramBase)
        {
            if (paramBase is not NodeDemoEntryParam param) 
                return;
            GraphRunner.SetOutPortVal(_node.OutIntParam1, param.IntParam1);
            GraphRunner.SetOutPortVal(_node.OutIntParam2, param.IntParam2);
        }

        public override void Execute()
        {
            Complete();
        }

        public override string GetNextNode()
        {
            return _nextNode;
        }
        
    }
}