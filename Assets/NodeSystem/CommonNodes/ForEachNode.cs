using System;
using System.Collections;
using GCL;

namespace NS
{
    //todo:目前没有在editor环境校验 InEnumerable和OutElement的类型匹配
    [Node("ForEach", "Common/FlowControl/ForEach", ENodeType.Action, typeof(ForEachFlowNodeRunner), CommonNodeCategory.FlowControl)]
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
        private IEnumerable _enumerable;
        private IEnumerator _enumerator;
        private string _outPortId;
        private bool _started;
        
        public override void Execute(NodeGraphRunner graphRunner, Node node)
        {
            base.Execute(graphRunner, node);
            var n = (ForEachNode)node;
            if (!_started)
            {
                _started = true;
                _enumerable = graphRunner.GetInPortVal<IEnumerable>(n.InEnumerable);
                if (_enumerable == null)
                {
                    GameLogger.LogError("foreach node failed. input is not valid.");
                    graphRunner.Abort();
                    return;
                }
                
                graphRunner.EnterLoop(this, node);
                _enumerator = _enumerable.GetEnumerator();
            }

            if (!_enumerator.MoveNext())
            {
                IsLoopEnd = true;
            }
            
            if (IsLoopEnd)
            {
                _outPortId = n.OutCompleteExecPort;
                if (_enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                graphRunner.ExitLoop();
            }
            else
            {
                graphRunner.SetOutPortVal(n.OutElement, _enumerator.Current);
                _outPortId = n.OutForEachExecPort;
            }

            graphRunner.Forward();
        }

        public override string GetNextNode(NodeGraphRunner graphRunner, Node node)
        {
            var outPort = graphRunner.GraphAssetRuntimeData.GetPortById(_outPortId);
            if (!outPort.IsConnected())
                return null;

            var connectPort = graphRunner.GraphAssetRuntimeData.GetPortById(outPort.connectPortId);
            return connectPort.belongNodeId;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            _started = false;
            _enumerable = null;
            if (_enumerable is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _enumerator = null;
        }
    }
}