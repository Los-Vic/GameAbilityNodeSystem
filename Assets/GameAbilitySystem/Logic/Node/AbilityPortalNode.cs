using NS;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Serialization;
using Node = NS.Node;

namespace GAS.Logic
{
    [Node("AbilityPortal", "GameAbilitySystem/Portal/AbilityPortal", ENodeFunctionType.Portal, typeof(AbilityPortalPortalNodeRunner), (int)ECommonNodeCategory.Portal, NodeScopeDefine.Ability)]
    public class AbilityPortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;

        [PortalType]
        public EAbilityPortal Portal;
        
        public override string DisplayName()
        {
            return Portal.ToString();
        }
    }
    
    public class AbilityPortalPortalNodeRunner:PortalNodeRunner
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