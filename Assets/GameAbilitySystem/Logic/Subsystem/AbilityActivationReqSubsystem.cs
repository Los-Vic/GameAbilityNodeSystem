using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public enum EActivationQueueType
    {
        NoQueue,
        Unit,
        Player,
        World
    }

    public struct AbilityCastCfg
    {
        public FP PreCastTime;
        public FP CastTime;
        public FP PostCastTime;
    }
    
    public enum EActivationReqJobState
    {
        Waiting,
        Running,
        Completed,
        Cancelled
    }

    public enum EActivationJobInCastState
    {
        None,
        PreCast,
        Cast,
        PostCast
    }
    
    public struct AbilityActivationReq
    {
        public GameAbility Ability;
        public GameEventArg EventArgs;
        public EActivationQueueType QueueType;
        public AbilityCastCfg CastCfg;
    }
    
    public class AbilityActivationReqJob:IRefCountRequester, IPoolClass
    {
        public AbilityActivationReq Req { get;private set; }
        public EActivationReqJobState JobState { get; private set; }
        public EActivationJobInCastState CastState { get; private set; }

        private bool _isValid;
        private FP _timeFromLastCastState;

        internal void InitJob(AbilityActivationReq req)
        {
            Req = req;
            Req.EventArgs?.GetRefCountDisposableComponent().AddRefCount(this);
        }

        internal void StartJob()
        {
            JobState = EActivationReqJobState.Running;
            ExecuteStartPreCast();
        }

        internal void CancelJob()
        {
            JobState = EActivationReqJobState.Cancelled;
            Req.EventArgs?.GetRefCountDisposableComponent().RemoveRefCount(this);
        }
        
        internal void TickJob(FP tickTime)
        {
            if(JobState != EActivationReqJobState.Running)
                return;

            _timeFromLastCastState += tickTime;
            switch (CastState)
            {
                case EActivationJobInCastState.PreCast:
                    if (_timeFromLastCastState >= Req.CastCfg.PreCastTime)
                    {
                        _timeFromLastCastState -= Req.CastCfg.PreCastTime;
                        ExecuteStartCast();
                    }
                    break;
                case EActivationJobInCastState.Cast:
                    if (_timeFromLastCastState >= Req.CastCfg.CastTime)
                    {
                        _timeFromLastCastState -= Req.CastCfg.CastTime;
                        ExecuteStartPostCast();
                    }
                    break;
                case EActivationJobInCastState.PostCast:
                    if (_timeFromLastCastState >= Req.CastCfg.PostCastTime)
                    {
                        _timeFromLastCastState -= Req.CastCfg.PostCastTime;
                        ExecuteEndPostCast();
                    }
                    break;
            }
        }

        private void ExecuteStartPreCast()
        {
            CastState = EActivationJobInCastState.PreCast;
            Req.Ability.ActivateOnStartPreCast(Req.EventArgs);
            if (Req.CastCfg.PreCastTime > 0)
                return;
            ExecuteStartCast();
        }
        
        private void ExecuteStartCast()
        {
            CastState = EActivationJobInCastState.Cast;
            Req.Ability.ActivateOnStartCast(Req.EventArgs);
            if(Req.CastCfg.CastTime > 0)
                return;
            ExecuteStartPostCast();
        }
        
        private void ExecuteStartPostCast()
        {
            CastState = EActivationJobInCastState.PostCast;
            Req.Ability.ActivateOnStartPostCast(Req.EventArgs);
            if(Req.CastCfg.PostCastTime > 0) 
                return;
            ExecuteEndPostCast();
        }
        
        private void ExecuteEndPostCast()
        {
            CastState = EActivationJobInCastState.None;
            Req.Ability.ActivateOnEndPostCast(Req.EventArgs);
            JobState = EActivationReqJobState.Completed;
        }
        
        public bool IsRequesterStillValid()
        {
            return _isValid;
        }

        public string GetRequesterDesc()
        {
            return $"ReqJob_{Req.Ability?.AbilityName}_{Req.Ability?.Owner.UnitName}";
        }

        public void OnCreateFromPool(ClassObjectPool pool)
        {
        }

        public void OnTakeFromPool()
        {
            _isValid = true;
        }

        public void OnReturnToPool()
        {
            Req.EventArgs?.GetRefCountDisposableComponent().RemoveRefCount(this);
            _isValid = false;
            CastState = EActivationJobInCastState.None;
            JobState = EActivationReqJobState.Waiting;
            _timeFromLastCastState = 0;
        }

        public void OnDestroy()
        {
        }
    }
    
    /// <summary>
    /// Job的结束
    /// </summary>
    public class AbilityActivationReqSubsystem:GameAbilitySubsystem
    {
        private readonly Queue<AbilityActivationReqJob> _worldQueue = new();
        private readonly Dictionary<int, Queue<AbilityActivationReqJob>> _playerQueues = new();
        private readonly Dictionary<GameUnit, Queue<AbilityActivationReqJob>> _unitQueues = new();
        private readonly List<AbilityActivationReqJob> _independentJobList = new();
        
        private readonly List<Queue<AbilityActivationReqJob>> _updateUnitQueueList = new();
        private readonly List<AbilityActivationReqJob> _traverseJobList = new();

        public override void UnInit()
        {
            base.UnInit();
        }

        public override void Update(float deltaTime)
        {
            //World 
            while (_worldQueue.Count > 0)
            {
                var job = _worldQueue.Peek();
                job.TickJob(deltaTime);
                if(job.JobState == EActivationReqJobState.Running)
                    break;
                _worldQueue.Dequeue();
                System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
            }
            //Player
            foreach (var queue in _playerQueues.Values)
            {
                while (queue.Count > 0)
                {
                    var job = queue.Peek();
                    job.TickJob(deltaTime);
                    if(job.JobState == EActivationReqJobState.Running)
                        break;
                    queue.Dequeue();
                    System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                }
            }
            //Unit

            //遍历的过程中有可能删除Unit
            _updateUnitQueueList.Clear();
            foreach (var queue in _unitQueues.Values)
            {
                _updateUnitQueueList.Add(queue);
            }
            
            foreach (var queue in _updateUnitQueueList)
            {
                while (queue.Count > 0)
                {
                    var job = queue.Peek();
                    job.TickJob(deltaTime);
                    if(job.JobState == EActivationReqJobState.Running)
                        break;
                    queue.Dequeue();
                    System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                }
            }
            
            //Independent
            _traverseJobList.Clear();
            foreach (var job in _independentJobList)
            {
                _traverseJobList.Add(job);
            }

            foreach (var job in _traverseJobList)
            {
                job.TickJob(deltaTime);
                if (job.JobState == EActivationReqJobState.Running)
                    continue;
                _independentJobList.Remove(job);
                System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
            }
        }

        internal void EnqueueJob(AbilityActivationReqJob job)
        {
            switch (job.Req.QueueType)
            {
               case EActivationQueueType.Unit:
                   var unit = job.Req.Ability.Owner;
                   if (_unitQueues.TryGetValue(unit, out var unitQueue))
                   {
                       if (unitQueue.Count > 0)
                       {
                           unitQueue.Enqueue(job);
                       }
                       else
                       {
                           job.StartJob();
                           if (job.JobState == EActivationReqJobState.Running)
                           {
                               unitQueue.Enqueue(job);
                           }
                           else
                           {
                               System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                           }
                       }
                   }
                   else
                   {
                       GameLogger.LogError($"fail to find unit job queue, unit:{unit.UnitName}");
                   }
                   break;
               case EActivationQueueType.Player:
                   var playerIndex = job.Req.Ability.Owner.PlayerIndex;
                   if (_playerQueues.TryGetValue(playerIndex, out var playerQueue))
                   {
                       if (playerQueue.Count > 0)
                       {
                           playerQueue.Enqueue(job);
                       }
                       else
                       {
                           job.StartJob();
                           if (job.JobState == EActivationReqJobState.Running)
                           {
                               playerQueue.Enqueue(job);
                           }
                           else
                           {
                               System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                           }
                       }
                   }
                   else
                   {
                       GameLogger.LogError($"fail to find player job queue, player:{playerIndex}");
                   }
                   break;
               case EActivationQueueType.World:
                   if (_worldQueue.Count > 0)
                   {
                       _worldQueue.Enqueue(job);
                   }
                   else
                   {
                       job.StartJob();
                       if (job.JobState == EActivationReqJobState.Running)
                       {
                           _worldQueue.Enqueue(job);
                       }
                       else
                       {
                           System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                       }
                   }
                   break;
               case EActivationQueueType.NoQueue:
                   job.StartJob();
                   if (job.JobState == EActivationReqJobState.Running)
                   {
                       _independentJobList.Add(job);
                   }
                   else
                   {
                       System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Release(job);
                   }
                   break;
            }
        }

        internal void CreatePlayerQueues(int playerCount)
        {
            for (var i = 0; i < playerCount; i++)
            {
                _playerQueues.Add(i, new Queue<AbilityActivationReqJob>());
            }
        }
        
        internal void CreateGameUnitQueue(GameUnit unit)
        {
            _unitQueues.TryAdd(unit, new Queue<AbilityActivationReqJob>());
        }

        internal void RemoveGameUnitQueue(GameUnit unit)
        {
            _unitQueues.Remove(unit);
        }
    }
    
    
}