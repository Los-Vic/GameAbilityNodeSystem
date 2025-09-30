using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace Gray.NG.Editor
{
    public class EntryNode:GraphNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            AddInputOutputPorts(typeof(RtEntryNode), context);
        }

        public override RuntimeNode CreateRuntimeNode()
        {
            return new RtEntryNode();
        }

        public override void AssignRuntimeNodePortValues(RuntimeNode runtimeNode, Dictionary<INode, RuntimeNode> nodeMap)
        {
            if(runtimeNode is not RtEntryNode rtEntryNode)
                return;

            rtEntryNode.EntryVal = GetInputPortValue<int>(GetInputPortByName("EntryVal"));

            var node = GetNextNode(this, "NextNode");
            if (node != null && nodeMap.TryGetValue(node, out var rtNode))
            {
                rtEntryNode.NextNode = rtNode;
            }
        }
    }
}