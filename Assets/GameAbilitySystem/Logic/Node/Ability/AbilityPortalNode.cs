using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    public class AbilityPortalNode : Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    [Node("OnAdd", "Ability/Portal/OnAdd", ENodeFunctionType.Portal, typeof(AbilityPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnAddAbilityPortalNode:AbilityPortalNode
    {
    }
    [Node("OnRemove", "Ability/Portal/OnRemove", ENodeFunctionType.Portal, typeof(AbilityPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnRemoveAbilityPortalNode:AbilityPortalNode
    {
    }
    
    public class OnActivateAbilityPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventArg")]
        public string OutPortParam;
    }

    [Node("OnStartPreCast", "Ability/Portal/OnStartPreCast", ENodeFunctionType.Portal,
        typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnStartPreCastAbilityPortalNode : OnActivateAbilityPortalNode
    {
    }
    
    [Node("OnStartCast", "Ability/Portal/OnStartCast", ENodeFunctionType.Portal,
        typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnStartCastAbilityPortalNode : OnActivateAbilityPortalNode
    {
    }
    
    [Node("OnStartPostCast", "Ability/Portal/OnStartPostCast", ENodeFunctionType.Portal,
        typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnStartPostCastAbilityPortalNode : OnActivateAbilityPortalNode
    {
    }
    
    [Node("OnEndPostCast", "Ability/Portal/OnEndPostCast", ENodeFunctionType.Portal,
        typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public sealed class OnEndPostCastAbilityPortalNode : OnActivateAbilityPortalNode
    {
    }
    
    
    public sealed class AbilityPortalNodeRunner:PortalNodeRunner
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

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _nextNode = null;
            _node = null;
        }
    }
    
    public sealed class AbilityGameEventPortalNodeRunner:PortalNodeRunner
    {
        private string _nextNode;
        private OnActivateAbilityPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (OnActivateAbilityPortalNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetPortalParam(IPortalParam paramBase)
        {
            if (paramBase == null)
                return;
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

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _nextNode = null;
            _node = null;
        }
    }
}