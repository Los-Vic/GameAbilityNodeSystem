using System;
using System.Collections.Generic;
using CommonObjectPool;

namespace NS
{
    public interface INodeSystemTaskScheduler
    {
        NodeTask CreateTask(string taskName, NodeGraphRunner runner,
            Func<ENodeSystemTaskRunStatus> startTask, Action endTask,
            Action cancelTask,
            Func<float, ENodeSystemTaskRunStatus> updateTask = null);
        
        void StartTask(NodeTask task);
        void ArrangeTaskUpdatePolicy(NodeTask task);
        void CancelTask(NodeTask task);
        void CancelTasksOfGraphRunner(NodeGraphRunner runner);
        bool HasTaskRunning(NodeGraphRunner runner);
        void UpdateScheduler(float dt);
        void DestroyTasks(NodeGraphRunner runner);
        void DestroyTask(NodeTask task);
        void EndTask(NodeTask task);
    }
    
    
    public class NodeTaskScheduler:INodeSystemTaskScheduler
    {
        protected readonly ObjectPoolMgr PoolMgr;

        private readonly List<NodeTask> _updateList = new();
        private readonly List<NodeTask> _traversalUpdateList = new();
        private readonly List<NodeTask> _pendingAddToUpdateList = new();

        private readonly Dictionary<NodeGraphRunner, List<NodeTask>> _graphRunnerTasksMap = new();
        private readonly Dictionary<NodeTask, NodeGraphRunner> _taskGraphRunnerMap = new();
        
        public NodeTaskScheduler(ObjectPoolMgr poolMgr)
        {
            PoolMgr = poolMgr;
        }

        public NodeTask CreateTask(string taskName, NodeGraphRunner runner, Func<ENodeSystemTaskRunStatus> startTask, Action endTask, Action cancelTask, 
            Func<float, ENodeSystemTaskRunStatus> updateTask = null)
        {
            var task = PoolMgr.CreateObject<NodeTask>();
            task.InitTask(taskName, startTask, endTask, cancelTask, updateTask);
            
            _graphRunnerTasksMap.TryAdd(runner, new List<NodeTask>());
            _graphRunnerTasksMap[runner].Add(task);
            _taskGraphRunnerMap.Add(task, runner);
            
            return task;
        }

        public void StartTask(NodeTask task)
        {
            NodeSystemLogger.Log($"start task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            var status = task.StartTask?.Invoke() ?? ENodeSystemTaskRunStatus.End;
            if (status == ENodeSystemTaskRunStatus.End || task.UpdateTask == null)
            {
                EndTask(task);
                return;
            }

            ArrangeTaskUpdatePolicy(task);
        }

        public void ArrangeTaskUpdatePolicy(NodeTask task)
        {
            _pendingAddToUpdateList.Add(task);
        }
        
        public void CancelTask(NodeTask task)
        {
            _updateList.Remove(task);
            _pendingAddToUpdateList.Remove(task);
            NodeSystemLogger.Log($"cancel task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            task.CancelTask?.Invoke();
            DestroyTask(task);
        }

        public void CancelTasksOfGraphRunner(NodeGraphRunner runner)
        {
            var cancelTaskSuccess = false;
            if (_graphRunnerTasksMap.TryGetValue(runner, out var tasks))
            {
                foreach (var t in tasks)
                {
                    _updateList.Remove(t);
                    _pendingAddToUpdateList.Remove(t);
                    NodeSystemLogger.Log($"cancel task, asset:{_taskGraphRunnerMap[t].AssetName}, event:{_taskGraphRunnerMap[t].EventName}");
                    t.CancelTask?.Invoke();
                    cancelTaskSuccess = true;
                }
            }

            if (cancelTaskSuccess)
            {
                runner.CancelRunner();
            }
            DestroyTasks(runner);
        }

        public bool HasTaskRunning(NodeGraphRunner runner)
        {
            return _graphRunnerTasksMap.ContainsKey(runner);
        }
        
        public void UpdateScheduler(float dt)
        {
            if(_pendingAddToUpdateList.Count == 0 && _updateList.Count == 0)
                return;
            
            _traversalUpdateList.AddRange(_updateList);
            foreach (var t in _traversalUpdateList)
            {
                var status = t.UpdateTask.Invoke(dt);
                if (status != ENodeSystemTaskRunStatus.End)
                    continue;
                EndTask(t);
            }
            _traversalUpdateList.Clear();
            
            if (_pendingAddToUpdateList.Count == 0) 
                return;
            _updateList.AddRange(_pendingAddToUpdateList);
            _pendingAddToUpdateList.Clear();
        }

        public void DestroyTasks(NodeGraphRunner runner)
        {
            if (!_graphRunnerTasksMap.Remove(runner, out var runnerTasks)) 
                return;
            
            foreach (var t in runnerTasks)
            {
                _taskGraphRunnerMap.Remove(t);
                PoolMgr.DestroyObject(t);
            }
        }
        
        public void DestroyTask(NodeTask task)
        {
            if (_taskGraphRunnerMap.Remove(task, out var runner))
            {
                if (_graphRunnerTasksMap.TryGetValue(runner, out var runnerTasks))
                {
                    runnerTasks.Remove(task);
                    if (runnerTasks.Count == 0)
                    {
                        _graphRunnerTasksMap.Remove(runner);
                    }
                }
            }
            PoolMgr.DestroyObject(task);
        }
        public void EndTask(NodeTask task)
        {
            _updateList.Remove(task);
            NodeSystemLogger.Log($"end task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            task.EndTask?.Invoke();
            DestroyTask(task);
        }

    }
}