using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    public class AbilityEntryNode : Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    [Node("OnAdd", "Ability/Entry/OnAdd", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnAddAbilityEntryNode:AbilityEntryNode
    {
    }
    [Node("OnRemove", "Ability/Entry/OnRemove", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnRemoveAbilityEntryNode:AbilityEntryNode
    {
    }
    
    public class OnActivateAbilityEntryNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventArg")]
        public string OutPortParam;
    }

    [Node("OnStartPreCast", "Ability/Entry/OnStartPreCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnStartPreCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnStartCast", "Ability/Entry/OnStartCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnStartCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnStartPostCast", "Ability/Entry/OnStartPostCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnStartPostCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnEndPostCast", "Ability/Entry/OnEndPostCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.Ability)]
    public sealed class OnEndPostCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    
    public sealed class AbilityEntryNodeRunner:EntryNodeRunner
    {
        private string _nextNode;
        private AbilityEntryNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (AbilityEntryNode)nodeAsset;
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
    
    public sealed class AbilityGameEventEntryNodeRunner:EntryNodeRunner
    {
        private string _nextNode;
        private OnActivateAbilityEntryNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (OnActivateAbilityEntryNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetEntryParam(IEntryParam paramBase)
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