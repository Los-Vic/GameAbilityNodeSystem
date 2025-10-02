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

        public override bool IsTickable()
        {
            return true;
        }

        public override void AssignRuntimeNodePortValues(RuntimeNode runtimeNode, NodeGraphImporter importer)
        {
            if(runtimeNode is not RtEntryNode rtEntryNode)
                return;

            rtEntryNode.EntryVal = GetInputPortValue<int>(GetInputPortByName("EntryVal"));

            var node = GetNextNode(this, "NextNode");
            if (node != null)
            {
                rtEntryNode.NextNode = importer.GetRuntimeNodeOfGraphNode(node);
            }
        }
    }
}