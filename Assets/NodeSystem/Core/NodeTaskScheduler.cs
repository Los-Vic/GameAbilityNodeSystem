using System;
using System.Collections.Generic;
using GameplayCommonLibrary;

namespace NS
{
    public interface INodeSystemTaskScheduler
    {
        NodeTask CreateTask(string taskName, NodeGraphRunner runner,
            Func<ETaskStatus> startTask, Action endTask,
            Action cancelTask,
            Func<float, ETaskStatus> updateTask = null);
        
        void StartTask(NodeTask task);
        void ArrangeTaskUpdatePolicy(NodeTask task);
        void ForceCancelTask(NodeTask task);
        void CancelTasksOfGraphRunner(NodeGraphRunner runner);
        bool HasTaskRunning(NodeGraphRunner runner);
        void UpdateScheduler(float dt);
    }
    
    
    public class NodeTaskScheduler:INodeSystemTaskScheduler
    {
        protected readonly ObjectPoolMgr PoolMgr;

        private readonly List<NodeTask> _allTasks = new List<NodeTask>();
        private readonly List<NodeTask> _updateList = new();
        private readonly List<NodeTask> _pendingAddToUpdateList = new();
        private readonly List<NodeTask> _pendingDestroyList = new();

        //销毁NodeGraphRunner时，能找到所拥有的NodeTask进行清理
        private readonly Dictionary<NodeGraphRunner, List<NodeTask>> _graphRunnerTasksMap = new();
        private readonly Dictionary<NodeTask, NodeGraphRunner> _taskGraphRunnerMap = new();
        
        public NodeTaskScheduler(ObjectPoolMgr poolMgr)
        {
            PoolMgr = poolMgr;
        }

        public NodeTask CreateTask(string taskName, NodeGraphRunner runner, Func<ETaskStatus> startTask, Action endTask, Action cancelTask, 
            Func<float, ETaskStatus> updateTask = null)
        {
            var task = PoolMgr.CreateObject<NodeTask>();
            _allTasks.Add(task);
            
            taskName = $"{taskName}_{runner.AssetName}_{runner.PortalName}";
            task.InitTask(taskName, startTask, endTask, cancelTask, updateTask);
            
            _graphRunnerTasksMap.TryAdd(runner, new List<NodeTask>());
            _graphRunnerTasksMap[runner].Add(task);
            _taskGraphRunnerMap.Add(task, runner);
            
            return task;
        }

        public void StartTask(NodeTask task)
        {
            var status = task.StartTask();
            if(status == ETaskStatus.Running)
                ArrangeTaskUpdatePolicy(task);
        }

        public void ArrangeTaskUpdatePolicy(NodeTask task)
        {
            _pendingAddToUpdateList.Add(task);
        }
        
        public void ForceCancelTask(NodeTask task)
        {
            task.CancelTask();
        }

        public void CancelTasksOfGraphRunner(NodeGraphRunner runner)
        {
            if (!_graphRunnerTasksMap.TryGetValue(runner, out var tasks)) 
                return;
            
            foreach (var t in tasks)
            {
                if(t.IsEnded)
                    continue;
                t.CancelTask();
            }
        }

        public bool HasTaskRunning(NodeGraphRunner runner)
        {
            return _graphRunnerTasksMap.ContainsKey(runner);
        }
        
        public void UpdateScheduler(float dt)
        {
            //Add pending tasks to update list
            if (_pendingAddToUpdateList.Count != 0)
            {
                _updateList.AddRange(_pendingAddToUpdateList);
                _pendingAddToUpdateList.Clear();
            }
            // Update tasks
            foreach (var t in _updateList)
            {
                t.UpdateTask(dt);
            }

            //Check task is ended?
            foreach (var t in _allTasks)
            {
               if(t.IsEnded)
                   _pendingDestroyList.Add(t);
            }

            //Clear ended tasks
            foreach (var t in _pendingDestroyList)
            {
                _allTasks.Remove(t);
                _pendingAddToUpdateList.Remove(t);
                _updateList.Remove(t);

                _taskGraphRunnerMap.Remove(t, out var runner);
                _graphRunnerTasksMap[runner].Remove(t);
                if(_graphRunnerTasksMap[runner].Count == 0)
                    _graphRunnerTasksMap.Remove(runner);
                
                PoolMgr.DestroyObject(t);
            }
            _pendingDestroyList.Clear();
        }

    }
}