using System;
using CommonObjectPool;

namespace NS
{
    public enum ENodeSystemTaskRunStatus
    {
        Running,
        End,
    }
    
    public class NodeSystemTask: IPoolObject
    {
        public string TaskName { get; private set; }
        internal Func<ENodeSystemTaskRunStatus> StartTask { get; private set; }
        internal Action EndTask { get; private set; }
        internal Action CancelTask { get; private set; }
        internal Func<float, ENodeSystemTaskRunStatus> UpdateTask { get; private set; }

        internal void InitTask(string taskName, Func<ENodeSystemTaskRunStatus> startTask, Action endTask, Action cancelTask, 
            Func<float, ENodeSystemTaskRunStatus> updateTask = null)
        {
            TaskName = taskName;
            StartTask = startTask;
            EndTask = endTask;
            CancelTask = cancelTask;
            UpdateTask = updateTask;
        }
        
        #region PoolObject
        public void OnCreateFromPool()
        {
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            StartTask = null;
            EndTask = null;
            CancelTask = null;
            UpdateTask = null;
        }

        public void OnDestroy()
        {
        }
        #endregion
    }
    
}