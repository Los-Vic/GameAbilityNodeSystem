using NS;
using UnityEditor.Experimental.GraphView;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("DefaultEvent", "GameAbilitySystem/Event/DefaultEvent", ENodeFunctionType.Event, typeof(DefaultEventEventNodeRunner))]
    public class DefaultEventNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [EventType]
        public EDefaultEvent NodeEvent;
        
        public override string DisplayName()
        {
            return NodeEvent.ToString();
        }
    }
    
    public class DefaultEventEventNodeRunner:EventNodeRunner
    {
        private string _nextNode;
        private GameEventNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (GameEventNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
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