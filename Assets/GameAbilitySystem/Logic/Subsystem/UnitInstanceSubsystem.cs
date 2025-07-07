using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler; 

namespace GAS.Logic
{
    /// <summary>
    /// Unit拥有Ability、Effect、Attribute对象，但销毁Unit时，会销毁所拥有的对象
    /// </summary>
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        public HandlerResourceMgr<GameUnit> UnitHandlerRscMgr { get; private set; }
        private readonly List<Handler<GameUnit>> _pendingDestroyUnitList = new();

        public override void Init()
        {
            base.Init();
            UnitHandlerRscMgr = new HandlerResourceMgr<GameUnit>(512);
        }

        public override void UnInit()
        {
            UnitHandlerRscMgr.ForeachResource(DisposeUnit);
            _pendingDestroyUnitList.Clear();
        }

        public override void Update(float deltaTime)
        {
            if (_pendingDestroyUnitList.Count == 0)
                return;

            for (var i = _pendingDestroyUnitList.Count - 1; i >= 0; i--)
            {
                var h = _pendingDestroyUnitList[i];
                if (UnitHandlerRscMgr.GetRefCount(h) != 0 || !UnitHandlerRscMgr.Dereference(h, out var unit)) 
                    continue;
                DisposeUnit(unit);
                _pendingDestroyUnitList.RemoveAt(i);
            }
        }

        public GameUnit[] GetAllUnits()
        {
            return UnitHandlerRscMgr.GetAllResources();
        }

        #region Game Unit Instance Create/Destroy
        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameUnit>();
            var h = UnitHandlerRscMgr.Create(unit);

            var initParam = new GameUnitInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            unit.Init(System, ref initParam);
            
            System.OnUnitCreated.NotifyObservers(new GameUnitCreateObserve()
            {
                Reason = param.Reason,
                Unit = unit
            });
            var context = new UnitCreateCueContext()
            {
                UnitInstanceID = unit.Handler
            };
            System.GameCueSubsystem.PlayUnitCreateCue(ref context);
            GameLogger.Log($"Create unit:{param.UnitName}, reason:{param.Reason}");
            return unit;
        }
        
        internal void DestroyGameUnit(Handler<GameUnit> unitHandler, EDestroyUnitReason reason = EDestroyUnitReason.None)
        {
            if (!UnitHandlerRscMgr.Dereference(unitHandler, out var unit))
            {
                GameLogger.LogError($"Destroy game unit failed, unit handler generation mismatch. {unitHandler}");
                return;
            }

            if (unit.Status is EUnitStatus.Destroyed or EUnitStatus.PendingDestroy)
            {
                GameLogger.Log($"Unit is marked as destroyed or destroyed, {unit}");
                return;
            }
            
            GameLogger.Log($"Destroy unit:{unit}, reason:{reason}");
            unit.MarkForDestroy(reason);
            var context = new UnitDestroyCueContext()
            {
                UnitInstanceID = unitHandler
            };
            System.GameCueSubsystem.PlayUnitDestroyCue(ref context);
            unit.OnUnitDestroyed.NotifyObservers(reason);
            System.OnUnitDestroyed.NotifyObservers(new GameUnitDestroyObserve()
            {
                Reason = reason,
                Unit = unit
            });

            if (UnitHandlerRscMgr.GetRefCount(unitHandler) > 0)
            {
                _pendingDestroyUnitList.Add(unitHandler);
            }
            else
            {
                DisposeUnit(unit);
            }
        }

        private void DisposeUnit(GameUnit unit)
        {
            System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Release(unit);
            UnitHandlerRscMgr.Release(unit.Handler);
        }

        #endregion
    }
}