using NS;
using UnityEditor.Experimental.GraphView;

namespace GAS.Logic
{
    [Node("DefaultEvent", "GameAbilitySystem/Event/DefaultEvent", ENodeCategory.Event, ENodeNumsLimit.None, typeof(DefaultEventNodeRunner))]
    public class DefaultEventNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;

        [EventType]
        public EDefaultEvent NodeEvent;
        
        public override string DisplayName()
        {
            return NodeEvent.ToString();
        }
    }
    
    public class DefaultEventNodeRunner:NodeSystemEventNodeRunner
    {
        private string _nextNode;
        private GameEventNode _node;
        
        public override void Init(NodeSystemNode nodeAsset, NodeSystemGraphRunner graphRunner)
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