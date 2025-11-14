using System;
using Gameplay.Common;

namespace NS
{
    public enum ETaskStatus
    {
        Dead,
        Waiting,
        Running,
        Completed,
        Cancelled
    }
    
    public class NodeTask: IPoolObject
    {
        private Func<ETaskStatus> OnStartTask { get; set; }
        private Action OnCompleteTask { get; set; }
        private Action OnCancelTask { get; set; }
        private Func<float, ETaskStatus> OnUpdateTask { get; set; }

        public string TaskName { get; private set; }
        public ETaskStatus Status { get; private set; }
        public bool IsEnded => Status is ETaskStatus.Completed or ETaskStatus.Cancelled;
        
        public void InitTask(string taskName, Func<ETaskStatus> startTask, Action completeTask, Action cancelTask, 
            Func<float, ETaskStatus> updateTask = null)
        {
            Status = ETaskStatus.Waiting;
            TaskName = taskName;
            OnStartTask = startTask;
            OnCompleteTask = completeTask;
            OnCancelTask = cancelTask;
            OnUpdateTask = updateTask;
        }
        
        public ETaskStatus StartTask()
        {
            if (OnStartTask == null || Status != ETaskStatus.Waiting)
            {
                GameLogger.LogWarning($"start task:{TaskName} failed! status:{Status}");
                return Status;
            }
            GameLogger.Log($"start task:{TaskName} succeeded!");
            var status = OnStartTask.Invoke();
            UpdateStatusFromDelegateResult(status);
            return Status;
        }

        public void UpdateTask(float deltaTime)
        {
            if (OnUpdateTask == null || Status != ETaskStatus.Running)
            {
                GameLogger.LogWarning($"update task:{TaskName} failed! status:{Status}");
                return;
            }
            var status = OnUpdateTask.Invoke(deltaTime);
            UpdateStatusFromDelegateResult(status);
        }
        
        public void CancelTask()
        {
            if (IsEnded)
            {
                GameLogger.LogWarning($"cancel task:{TaskName} failed! already ended, status:{Status}!");
                return;
            }
            GameLogger.Log($"cancel task:{TaskName} succeeded!");
            Status = ETaskStatus.Cancelled;
            OnCancelTask?.Invoke();
        }

        private void UpdateStatusFromDelegateResult(ETaskStatus newStatus)
        {
            switch (newStatus)
            {
                case ETaskStatus.Completed:
                    CompleteTask();
                    break;
                case ETaskStatus.Cancelled:
                    CancelTask();
                    break;
                default:  
                    Status = newStatus;
                    break;
            }
        }
        
        private void CompleteTask()
        {
            if (IsEnded)
            {
                GameLogger.LogWarning($"complete task:{TaskName} failed! already ended, status:{Status}!");
                return;
            }
            GameLogger.Log($"complete task:{TaskName} succeeded!");
            Status = ETaskStatus.Completed;
            OnCompleteTask?.Invoke();
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
            Status = ETaskStatus.Dead;
            TaskName = string.Empty;
            OnStartTask = null;
            OnCompleteTask = null;
            OnCancelTask = null;
            OnUpdateTask = null;
        }

        public virtual void OnDestroy()
        {
        }
        #endregion
    }
    
}