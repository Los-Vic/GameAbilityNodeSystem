using NS;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Serialization;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("DefaultPortal", "GameAbilitySystem/Portal/DefaultPortal", ENodeFunctionType.Portal, typeof(DefaultPortalPortalNodeRunner), (int)ECommonNodeCategory.Portal, NodeScopeDefine.Ability)]
    public class DefaultPortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [PortalType]
        public EDefaultPortal NodePortal;
        
        public override string DisplayName()
        {
            return NodePortal.ToString();
        }
    }
    
    public class DefaultPortalPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private GamePortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (GamePortalNode)nodeAsset;
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