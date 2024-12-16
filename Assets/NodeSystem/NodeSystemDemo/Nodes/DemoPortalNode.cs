using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Serialization;

namespace NS
{
    public enum ENodeDemoPortalType
    {
        BeginPlay,
        EndPlay
    }
    
    [Serializable]
    public class NodeDemoPortalParam:PortalParamBase
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    [Node("DemoPortalEvent", "Demo/Portal/DemoPortalEvent", ENodeFunctionType.Portal, typeof(PortalPortalNodeRunner), (int)ECommonNodeCategory.Portal, -1)]
    public class DemoPortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [FormerlySerializedAs("NodeEvent")] [PortalType]
        public ENodeDemoPortalType nodeDemoPortal;

        [Port(Direction.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(Direction.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return nodeDemoPortal.ToString();
        }
    }
    
    public class PortalPortalNodeRunner:PortalNodeRunner
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

        public override void SetPortalParam(PortalParamBase paramBase)
        {
            if (paramBase is not NodeDemoPortalParam param) 
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