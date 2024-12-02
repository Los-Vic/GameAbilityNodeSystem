using System;
using System.Collections.Generic;
using CommonObjectPool;

namespace NS
{
    public interface INodeSystemTaskScheduler
    {
        NodeSystemTask CreateTask(string taskName, NodeSystemGraphRunner runner,
            Func<ENodeSystemTaskRunStatus> startTask, Action endTask,
            Action cancelTask,
            Func<float, ENodeSystemTaskRunStatus> updateTask = null);

        void StartTask(NodeSystemTask task);
        void CancelTask(NodeSystemTask task);
        void CancelTasksOfGraphRunner(NodeSystemGraphRunner runner);
        bool HasTaskRunning(NodeSystemGraphRunner runner);
        void UpdateScheduler(float dt);
        void DestroyTasks(NodeSystemGraphRunner runner);
        void DestroyTask(NodeSystemTask task);
        void EndTask(NodeSystemTask task);
    }
    
    
    public class NodeSystemTaskScheduler:INodeSystemTaskScheduler
    {
        private readonly ObjectPoolMgr _poolMgr;

        private readonly List<NodeSystemTask> _updateList = new();
        private readonly List<NodeSystemTask> _traversalUpdateList = new();
        private readonly List<NodeSystemTask> _pendingAddToUpdateList = new();

        private readonly Dictionary<NodeSystemGraphRunner, List<NodeSystemTask>> _graphRunnerTasksMap = new();
        private readonly Dictionary<NodeSystemTask, NodeSystemGraphRunner> _taskGraphRunnerMap = new();
        
        public NodeSystemTaskScheduler(ObjectPoolMgr poolMgr)
        {
            _poolMgr = poolMgr;
        }

        public NodeSystemTask CreateTask(string taskName, NodeSystemGraphRunner runner, Func<ENodeSystemTaskRunStatus> startTask, Action endTask, Action cancelTask, 
            Func<float, ENodeSystemTaskRunStatus> updateTask = null)
        {
            var task = _poolMgr.CreateObject<NodeSystemTask>();
            task.InitTask(taskName, startTask, endTask, cancelTask, updateTask);
            
            _graphRunnerTasksMap.TryAdd(runner, new List<NodeSystemTask>());
            _graphRunnerTasksMap[runner].Add(task);
            _taskGraphRunnerMap.Add(task, runner);
            
            return task;
        }

        public void StartTask(NodeSystemTask task)
        {
            NodeSystemLogger.Log($"start task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            var status = task.StartTask?.Invoke() ?? ENodeSystemTaskRunStatus.End;
            if (status == ENodeSystemTaskRunStatus.End || task.UpdateTask == null)
            {
                EndTask(task);
                return;
            }
            _pendingAddToUpdateList.Add(task);
        }
        
        public void CancelTask(NodeSystemTask task)
        {
            _updateList.Remove(task);
            _pendingAddToUpdateList.Remove(task);
            NodeSystemLogger.Log($"cancel task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            task.CancelTask?.Invoke();
            DestroyTask(task);
        }

        public void CancelTasksOfGraphRunner(NodeSystemGraphRunner runner)
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

        public bool HasTaskRunning(NodeSystemGraphRunner runner)
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

        public void DestroyTasks(NodeSystemGraphRunner runner)
        {
            if (!_graphRunnerTasksMap.Remove(runner, out var runnerTasks)) 
                return;
            
            foreach (var t in runnerTasks)
            {
                _taskGraphRunnerMap.Remove(t);
                _poolMgr.DestroyObject(t);
            }
        }
        
        public void DestroyTask(NodeSystemTask task)
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
            _poolMgr.DestroyObject(task);
        }
        public void EndTask(NodeSystemTask task)
        {
            _updateList.Remove(task);
            NodeSystemLogger.Log($"end task, asset:{_taskGraphRunnerMap[task].AssetName}, event:{_taskGraphRunnerMap[task].EventName}");
            task.EndTask?.Invoke();
            DestroyTask(task);
        }

    }
}