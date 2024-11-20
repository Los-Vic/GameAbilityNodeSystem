using System.Collections.Generic;

namespace NS
{
    public class NodeSystemTaskScheduler
    {
        public readonly Dictionary<int, Queue<INodeSystemTask>> _taskQueueMap = new();


        public void EnqueueTask(INodeSystemTask task)
        {
            var priority = task.GetPriority();
            
        }

        public void UpdateScheduler(float deltaTime)
        {
            
        }
    }
}