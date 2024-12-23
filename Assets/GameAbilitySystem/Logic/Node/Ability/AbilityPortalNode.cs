using NS;
using UnityEditor.Experimental.GraphView;
using Node = NS.Node;

namespace GAS.Logic
{
    public class AbilityPortalNode : Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    [Node("OnAddAbility", "Ability/Portal/OnAddAbility", ENodeFunctionType.Portal, typeof(AbilityPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public class OnAddAbilityPortalNode:AbilityPortalNode
    {
    }
    [Node("OnRemoveAbility", "Ability/Portal/OnRemoveAbility", ENodeFunctionType.Portal, typeof(AbilityPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public class OnRemoveAbilityPortalNode:AbilityPortalNode
    {
    }
    [Node("OnActivateAbility", "Ability/Portal/OnActivateAbility", ENodeFunctionType.Portal, typeof(AbilityPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public class OnActivateAbilityPortalNode:AbilityPortalNode
    {
    }
    
    [Node("OnActivateAbilityEvent", "Ability/Portal/OnActivateAbilityEvent", ENodeFunctionType.Portal, typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public class OnActivateAbilityEventPortalNode:Node
    {
        [Port(Direction.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(Direction.Output, typeof(GameEventNodeParam), "EventParam")]
        public string OutPortParam;
    }
    
    public class AbilityPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private AbilityPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilityPortalNode)nodeAsset;
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
    
    public class AbilityGameEventPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private OnActivateAbilityEventPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (OnActivateAbilityEventPortalNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetPortalParam(PortalParamBase paramBase)
        {
           GraphRunner.SetOutPortVal(_node.OutPortParam, paramBase);
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