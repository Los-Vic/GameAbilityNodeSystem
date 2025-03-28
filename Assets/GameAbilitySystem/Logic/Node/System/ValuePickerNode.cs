using GAS.Logic.Value;
using MissQ;
using NS;
using UnityEngine;

namespace GAS.Logic
{
    [Node("ValuePicker", "System/Value/ValuePicker", ENodeFunctionType.Value , typeof(ValuePickerNodeRunner), CommonNodeCategory.Value, NodeScopeDefine.System)]
    public sealed class ValuePickerNode:Node
    {
        [Exposed]
        [SerializeReference]
        public ValuePickerBase Config;

        [Port(EPortDirection.Input, typeof(GameUnit), "Unit")]
        public string InPortUnit;
        
        [Port(EPortDirection.Input, typeof(FP), "Lv(opt)")]
        public string InPortLv;
        
        [Port(EPortDirection.Output, typeof(FP),"Val")]
        public string OutPortVal;
    }

    public sealed class ValuePickerNodeRunner : NodeRunner
    {
        private GameUnit _unit;
        private ValuePickerNode _node;
        
        public override void Init(Node nodeAsset, NodeGraphRunner graphRunner)
        {
            base.Init(nodeAsset, graphRunner);
            _node = (ValuePickerNode)nodeAsset;
            
            _unit = GraphRunner.GetInPortVal<GameUnit>(_node.InPortUnit);
        }

        public override void Execute()
        {
            var lv = GraphRunner.GetInPortVal<FP>(_node.InPortLv);
            GraphRunner.SetOutPortVal(_node.OutPortVal, ValuePickerUtility.GetValue(_node.Config, _unit, (uint)lv));
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _unit = null;
            _node = null;
        }
    }
}