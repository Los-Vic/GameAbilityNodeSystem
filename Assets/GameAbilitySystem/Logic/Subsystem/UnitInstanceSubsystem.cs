using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler; 

namespace GAS.Logic
{
    /// <summary>
    /// Unit拥有Ability、Effect、Attribute对象，但销毁Unit时，会销毁所拥有的对象
    /// </summary>
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        public HandlerRscMgr<GameUnit> UnitHandlerRscMgr { get; private set; }
        
        public override void Init()
        {
            base.Init();
            UnitHandlerRscMgr = new HandlerRscMgr<GameUnit>(512, DisposeUnit);
        }

        public override void UnInit()
        {
            //UnitHandlerRscMgr.ForeachResource(DisposeUnit);
            //UnitHandlerRscMgr.Reset();
        }

        public GameUnit[] GetAllUnits()
        {
            return UnitHandlerRscMgr.GetAllResources();
        }

        #region Game Unit Instance Create/Destroy
        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = System.ClassObjectPoolSubsystem.Get<GameUnit>();
            var h = UnitHandlerRscMgr.CreateHandler(unit);

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
            
            UnitHandlerRscMgr.RemoveRefCount(unitHandler);
        }

        private void DisposeUnit(GameUnit unit)
        {
            System.ClassObjectPoolSubsystem.Release(unit);
        }

        #endregion
    }
}