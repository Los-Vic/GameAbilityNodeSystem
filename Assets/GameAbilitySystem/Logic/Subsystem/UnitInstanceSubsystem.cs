using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        public override void UnInit()
        {
            var units = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.GetActiveObjects(typeof(GameUnit));
            if(units == null)
                return;
            foreach (var u in units.ToArray())
            {
                DestroyGameUnit((GameUnit)u);
            }
        }

        #region Game Unit Instance Create/Destroy

        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = System.GetSubsystem<ClassObjectPoolSubsystem>().ClassObjectPoolMgr.Get<GameUnit>();
            unit.Init(ref param);
            GameLogger.Log($"Create unit:{param.UnitName}!");
            return unit;
        }

        internal void DestroyGameUnit(GameUnit unit, EDestroyUnitReason reason = EDestroyUnitReason.Code)
        {
            GameLogger.Log($"Destroy unit:{unit.UnitName}, reason:{reason}");
            unit.DestroyReason = reason;
            unit.OnUnitDestroyed.NotifyObservers(reason);
            unit.GetRefCountDisposableComponent().MarkForDispose();
        }

        #endregion
    }
}