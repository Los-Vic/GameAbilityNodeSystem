using System;
using System.Collections;
using GameplayCommonLibrary;

namespace NS
{
    //todo:目前没有在editor环境校验 InEnumerable和OutElement的类型匹配
    [Node("ForEach", "Common/FlowControl/ForEach", ENodeFunctionType.Action, typeof(ForEachFlowNodeRunner), CommonNodeCategory.FlowControl)]
    public sealed class ForEachNode:Node
    {
        [Port(EPortDirection.Input, typeof(BaseFlowPort))]
        public string InExecPort;

        [Port(EPortDirection.Input, typeof(IEnumerable), "Enumerable")]
        public string InEnumerable;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort), "Completed")]
        public string OutCompleteExecPort;
        
        [Port(EPortDirection.Output, typeof(BaseFlowPort),"ForEach")]
        public string OutForEachExecPort;
        
        [Port(EPortDirection.Output, typeof(object), "Element")]
        public string OutElement;
    }
    
    public sealed class ForEachFlowNodeRunner:LoopNodeRunner
    {
        private ForEachNode _node;
        private IEnumerable _enumerable;
        private IEnumerator _enumerator;
        private string _outPortId;
        private bool _started;

        public override void Init(ref NodeRunnerInitContext context)
        {
            base.Init(ref context);
            _node = (ForEachNode)context.Node;
        }
        
        public override void Execute()
        {
            base.Execute();
            if (!_started)
            {
                _started = true;
                _enumerable = GraphRunner.GetInPortVal<IEnumerable>(_node.InEnumerable);
                if (_enumerable == null)
                {
                    GameLogger.LogError("foreach node failed. input is not valid.");
                    Abort();
                    return;
                }
                
                GraphRunner.EnterLoop(this);
                _enumerator = _enumerable.GetEnumerator();
            }

            if (!_enumerator.MoveNext())
            {
                IsLoopEnd = true;
            }
            
            if (IsLoopEnd)
            {
                _outPortId = _node.OutCompleteExecPort;
                if (_enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                GraphRunner.ExitLoop();
            }
            else
            {
                GraphRunner.SetOutPortVal(_node.OutElement, _enumerator.Current);
                _outPortId = _node.OutForEachExecPort;
            }

            Complete();
        }

        public override string GetNextNode()
        {
            var outPort = GraphRunner.GraphAssetRuntimeData.GetPortById(_outPortId);
            if (!outPort.IsConnected())
                return null;

            var connectPort = GraphRunner.GraphAssetRuntimeData.GetPortById(outPort.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _started = false;
            _node = null;
            _enumerable = null;
            if (_enumerable is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _enumerator = null;
        }
    }
}