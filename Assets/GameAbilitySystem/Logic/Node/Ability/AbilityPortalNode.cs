﻿using NS;
using Node = NS.Node;

namespace GAS.Logic
{
    public class AbilityPortalNode : Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
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
    
    [Node("OnActivateAbilityByEvent", "Ability/Portal/OnActivateAbilityByEvent", ENodeFunctionType.Portal, typeof(AbilityGameEventPortalNodeRunner), NodeCategoryDefine.AbilityEffectPortal, NodeScopeDefine.Ability)]
    public class OnActivateAbilityByEventPortalNode:Node
    {
        [Port(EPortDirection.Output, typeof(BaseFlowPort))]
        public string OutPortExec;
        [Port(EPortDirection.Output, typeof(GameEventArg), "EventParam")]
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
        private OnActivateAbilityByEventPortalNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            _node = (OnActivateAbilityByEventPortalNode)nodeAsset;
            var port = graphRunner.GraphAssetRuntimeData.GetPortById(_node.OutPortExec);
            if(!port.IsConnected())
                return;
            
            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(port.connectPortId);
            _nextNode = connectPort.belongNodeId;
        }

        public override void SetPortalParam(IPortalParam paramBase)
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