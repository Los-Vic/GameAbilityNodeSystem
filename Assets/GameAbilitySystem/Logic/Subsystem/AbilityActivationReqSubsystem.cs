using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;
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
        Cancelled,
        Aborted,
    }

    public enum EActivationJobInCastState
    {
        None,
        PreCast,
        Cast,
        PostCast,
    }

    public struct AbilityActivationReq
    {
        public Handler<GameAbility> Ability;
        public Handler<GameEventArg> EventArgs;
        public EActivationQueueType QueueType;
        public AbilityCastCfg CastCfg;
    }

    public class AbilityActivationReqJob : GameAbilitySystemObject
    {
        public AbilityActivationReq Req { get; private set; }
        public EActivationReqJobState JobState { get; private set; }
        public EActivationJobInCastState CastState { get; private set; }

        private FP _timeFromLastCastState;

        internal void InitJob(AbilityActivationReq req)
        {
            Req = req;
            System.GameEventSubsystem.GameEventRscMgr.AddRefCount(req.EventArgs);
        }

        internal void StartJob()
        {
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                GameLogger.LogWarning($"Start Job failed, failed to get ability {Req.Ability}");
                JobState = EActivationReqJobState.Aborted;
                return;
            }

            GameLogger.Log($"Start activation job: {ability}");
            JobState = EActivationReqJobState.Running;
            ExecuteStartPreCast();
        }

        internal void CancelJob()
        {
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                AbortJobForFailToGetAbility();
                return;
            }

            GameLogger.Log($"Cancel activation job: {ability}");
            JobState = EActivationReqJobState.Cancelled;
        }

        private void AbortJobForFailToGetAbility()
        {
            GameLogger.LogWarning($"Abort job, failed to get ability {Req.Ability}");
            JobState = EActivationReqJobState.Aborted;
        }
        
        internal void TickJob(FP tickTime)
        {
            if (JobState != EActivationReqJobState.Running)
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
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                AbortJobForFailToGetAbility();
                return;
            }

            CastState = EActivationJobInCastState.PreCast;
            System.GetRscFromHandler(Req.EventArgs, out var eventArg);
            ability.ActivateOnStartPreCast(eventArg);
            if (Req.CastCfg.PreCastTime > 0)
                return;
            ExecuteStartCast();
        }

        private void ExecuteStartCast()
        {
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                AbortJobForFailToGetAbility();
                return;
            }

            CastState = EActivationJobInCastState.Cast;
            System.GetRscFromHandler(Req.EventArgs, out var eventArg);
            ability.ActivateOnStartCast(eventArg);
            if (Req.CastCfg.CastTime > 0)
                return;
            ExecuteStartPostCast();
        }

        private void ExecuteStartPostCast()
        {
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                AbortJobForFailToGetAbility();
                return;
            }

            CastState = EActivationJobInCastState.PostCast;
            System.GetRscFromHandler(Req.EventArgs, out var eventArg);
            ability.ActivateOnStartPostCast(eventArg);
            if (Req.CastCfg.PostCastTime > 0)
                return;
            ExecuteEndPostCast();
        }

        private void ExecuteEndPostCast()
        {
            if (!System.GetRscFromHandler(Req.Ability, out var ability))
            {
                AbortJobForFailToGetAbility();
                return;
            }

            CastState = EActivationJobInCastState.None;
            System.GetRscFromHandler(Req.EventArgs, out var eventArg);
            ability.ActivateOnEndPostCast(eventArg);
            JobState = EActivationReqJobState.Completed;
            ability.RemoveActivationReqJob(this);

            GameLogger.Log($"Complete activation job: {ability}");
        }
        
        public override void OnReturnToPool()
        {
            System.GameEventSubsystem.GameEventRscMgr.RemoveRefCount(Req.EventArgs);
            CastState = EActivationJobInCastState.None;
            JobState = EActivationReqJobState.Waiting;
            _timeFromLastCastState = 0;
        }
    }

    /// <summary>
    /// Job的结束
    /// </summary>
    public class AbilityActivationReqSubsystem : GameAbilitySubsystem
    {
        private readonly Queue<AbilityActivationReqJob> _worldQueue = new();
        private readonly Dictionary<int, Queue<AbilityActivationReqJob>> _playerQueues = new();
        private readonly Dictionary<GameUnit, Queue<AbilityActivationReqJob>> _unitQueues = new();
        private readonly List<AbilityActivationReqJob> _independentJobList = new();

        private readonly List<Queue<AbilityActivationReqJob>> _updateUnitQueueList = new();
        private readonly List<AbilityActivationReqJob> _traverseJobList = new();

        public override void Init()
        {
            for (var i = 0; i < System.PlayerNums; i++)
            {
                _playerQueues.Add(i, new Queue<AbilityActivationReqJob>());
            }
        }

        public override void UnInit()
        {
            _worldQueue.Clear();
            _playerQueues.Clear();
            _unitQueues.Clear();
            _independentJobList.Clear();
            _updateUnitQueueList.Clear();
            _traverseJobList.Clear();
        }

        public override void Update(float deltaTime)
        {
            //World 
            while (_worldQueue.Count > 0)
            {
                var job = _worldQueue.Peek();
                job.TickJob(deltaTime);
                if (job.JobState == EActivationReqJobState.Running)
                    break;
                _worldQueue.Dequeue();
                System.ClassObjectPoolSubsystem.Release(job);
            }

            //Player
            foreach (var queue in _playerQueues.Values)
            {
                while (queue.Count > 0)
                {
                    var job = queue.Peek();
                    job.TickJob(deltaTime);
                    if (job.JobState == EActivationReqJobState.Running)
                        break;
                    queue.Dequeue();
                    System.ClassObjectPoolSubsystem.Release(job);
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
                    if (job.JobState == EActivationReqJobState.Running)
                        break;
                    queue.Dequeue();
                    System.ClassObjectPoolSubsystem.Release(job);
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
                System.ClassObjectPoolSubsystem.Release(job);
            }
        }

        internal void EnqueueJob(AbilityActivationReqJob job)
        {
            switch (job.Req.QueueType)
            {
                case EActivationQueueType.Unit:
                {
                    if (!System.GetRscFromHandler(job.Req.Ability, out var ability))
                        break;

                    if (!System.GetRscFromHandler(ability.Owner, out var unit))
                        break;

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
                                System.ClassObjectPoolSubsystem.Release(job);
                            }
                        }
                    }
                    else
                    {
                        GameLogger.LogError($"fail to find unit job queue, unit:{unit}");
                    }

                    break;
                }
                case EActivationQueueType.Player:
                {
                    if (!System.GetRscFromHandler(job.Req.Ability, out var ability))
                        break;
                    if (!System.GetRscFromHandler(ability.Owner, out var owner))
                        break;
                    var playerIndex = owner.PlayerIndex;
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
                                System.ClassObjectPoolSubsystem.Release(job);
                            }
                        }
                    }
                    else
                    {
                        GameLogger.LogError($"fail to find player job queue, player:{playerIndex}");
                    }

                    break;
                }
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
                            System.ClassObjectPoolSubsystem.Release(job);
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
                        System.ClassObjectPoolSubsystem.Release(job);
                    }

                    break;
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