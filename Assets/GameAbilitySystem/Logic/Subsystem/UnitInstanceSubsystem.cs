using GCL;

namespace GAS.Logic
{
    /// <summary>
    /// Unit拥有Ability、Effect、Attribute对象，但销毁Unit时，会销毁所拥有的对象
    /// </summary>
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        public override void Init()
        {
            base.Init();
            System.HandlerManagers.UnitHandlerMgr.Init(GetUnit, DisposeUnit, 512);
        }

        public override void UnInit()
        {
            System.HandlerManagers.UnitHandlerMgr.UnInit();
        }

        public Handler<GameUnit>[] GetAllUnits(out uint nums)
        {
            return System.HandlerManagers.UnitHandlerMgr.GetAllRscHandlers(out nums);
        }

        #region Game Unit Instance Create/Destroy
        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var h = System.HandlerManagers.UnitHandlerMgr.CreateHandler();
            System.HandlerManagers.UnitHandlerMgr.DeRef(h, out var unit);
            
            var initParam = new GameUnitInitParam()
            {
                CreateParam = param,
                Handler = h
            };
            unit.Init(ref initParam);
            
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
        
        internal void DestroyGameUnit(Handler<GameUnit> unitHandler, EDestroyUnitReason reason = EDestroyUnitReason.None, bool ignoreRefCount = false)
        {
            if (!System.HandlerManagers.UnitHandlerMgr.DeRef(unitHandler, out var unit))
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
            
            System.HandlerManagers.UnitHandlerMgr.RemoveRefCount(unitHandler);
        }

        private GameUnit GetUnit()
        {
            return System.ClassObjectPoolSubsystem.Get<GameUnit>();
        }
        
        private void DisposeUnit(GameUnit unit)
        {
            GameLogger.Log($"Release unit:{unit}");
            System.ClassObjectPoolSubsystem.Release(unit);
        }

        #endregion
    }
}