using UnityEditor.Experimental.GraphView;

namespace NS
{
    [Node("NodeEvent", "Default/Event/NodeEvent", ENodeCategory.Event, ENodeNumsLimit.None, typeof(EventNodeRunner))]
    public class EventNode:NodeSystemNode
    {
        [Port(Direction.Output, typeof(ExecutePort))]
        public string OutPortExec;

        [EventType]
        public ENodeEventType NodeEvent;

        [Port(Direction.Output, typeof(int), "IntParam1")]
        public string OutIntParam1;
        [Port(Direction.Output, typeof(int), "IntParam2")]
        public string OutIntParam2;

        public override string DisplayName()
        {
            return NodeEvent.ToString();
        }
    }
}