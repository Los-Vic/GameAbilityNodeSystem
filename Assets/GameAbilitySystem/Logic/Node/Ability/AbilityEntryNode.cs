using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    public class AbilityEntryNode : Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
    }
    
    [Node("OnAdd", "AbilitySystem/Entry/OnAdd", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnAddAbilityEntryNode:AbilityEntryNode
    {
    }
    [Node("OnRemove", "AbilitySystem/Entry/OnRemove", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnRemoveAbilityEntryNode:AbilityEntryNode
    {
    }

    [Node("OnTick","AbilitySystem/Entry/OnTick", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnTickAbilityEntryNode : AbilityEntryNode
    {
    }

    [Node("OnInstigatorDestroy","AbilitySystem/Entry/OnInstigatorDestroy", ENodeFunctionType.Entry, typeof(AbilityEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnInstigatorDestroyNode : AbilityEntryNode
    {
        
    }
    
    public class OnActivateAbilityEntryNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventArg")]
        public string OutPortParam;
    }

    [Node("OnStartPreCast", "AbilitySystem/Entry/OnStartPreCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnStartPreCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnStartCast", "AbilitySystem/Entry/OnStartCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnStartCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnStartPostCast", "AbilitySystem/Entry/OnStartPostCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnStartPostCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    [Node("OnEndPostCast", "AbilitySystem/Entry/OnEndPostCast", ENodeFunctionType.Entry,
        typeof(AbilityGameEventEntryNodeRunner), NodeCategoryDefine.AbilityEntry, NodeScopeDefine.AbilitySystem)]
    public sealed class OnEndPostCastAbilityEntryNode : OnActivateAbilityEntryNode
    {
    }
    
    
    public sealed class AbilityEntryNodeRunner:EntryNodeRunner
    {
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            if(node is not AbilityEntryNode n)
                return null;
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
    
    public sealed class AbilityGameEventEntryNodeRunner:EntryNodeRunner
    {
        public override void SetEntryParam(NodeGraphRunner graphRunner, Node node, IEntryParam paramBase)
        {
            if (paramBase == null)
                return;
            if(node is not OnActivateAbilityEntryNode n)
                return;
            graphRunner.SetOutPortVal(n.OutPortParam, paramBase);
        }

        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            if (node is not OnActivateAbilityEntryNode n)
            {
                return null;
            }
            
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(n.OutPortExec);
            if(!port.IsConnected())
                return null;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            return connectPort.belongNodeId;
        }
    }
}