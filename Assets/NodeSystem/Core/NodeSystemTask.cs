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
        public Func<ENodeSystemTaskRunStatus> StartTask { get; private set; }
        public Action EndTask { get; private set; }
        public Action CancelTask { get; private set; }
        public Func<float, ENodeSystemTaskRunStatus> UpdateTask { get; private set; }

        public void InitTask(string taskName, Func<ENodeSystemTaskRunStatus> startTask, Action endTask, Action cancelTask, 
            Func<float, ENodeSystemTaskRunStatus> updateTask = null)
        {
            TaskName = taskName;
            StartTask = startTask;
            EndTask = endTask;
            CancelTask = cancelTask;
            UpdateTask = updateTask;
        }
        
        #region PoolObject
        public virtual void OnCreateFromPool()
        {
        }

        public virtual void OnTakeFromPool()
        {
        }

        public virtual void OnReturnToPool()
        {
            StartTask = null;
            EndTask = null;
            CancelTask = null;
            UpdateTask = null;
        }

        public virtual void OnDestroy()
        {
        }
        #endregion
    }
    
}