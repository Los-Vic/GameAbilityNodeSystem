using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        #region Game Unit Instance Create/Destroy

        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = System.GetSubsystem<ObjectPoolSubsystem>().ObjectPoolMgr.Get<GameUnit>();
            unit.Init(ref param);
            GameLogger.Log($"Create unit:{param.UnitName}!");
            return unit;
        }

        internal void DestroyGameUnit(GameUnit unit)
        {
            GameLogger.Log($"Destroy unit:{unit.UnitName}!");
            unit.GetRefCountDisposableComponent().MarkForDispose();
        }

        #endregion
    }
}