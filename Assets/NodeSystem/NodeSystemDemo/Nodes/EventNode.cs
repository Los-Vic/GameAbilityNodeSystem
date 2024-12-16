using System;
using NS.Nodes;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    public enum ENodeEventType
    {
        BeginPlay,
        EndPlay
    }
    
    [Serializable]
    public class NodeEventParam:NodeSystemEventParamBase
    {
        public int IntParam1;
        public int IntParam2;
    }
    
    [Node("NodeEvent", "Demo/Event/NodeEvent", ENodeFunctionType.Event, typeof(EventEventNodeRunner), (int)ENodeCategory.Event)]
    public class EventNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [EventType]
        public ENodeEventType NodeEvent;

        [Port(Direction.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(Direction.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return NodeEvent.ToString();
        }
    }
    
    public class EventEventNodeRunner:EventNodeRunner
    {
        private string _nextNode;
        private EventNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (EventNode)nodeAsset;
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetUpEventParam(NodeSystemEventParamBase paramBase)
        {
            if (paramBase is not NodeEventParam param) 
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