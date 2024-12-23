using NS;

namespace GAS.Logic
{
    public class GameUnitInstanceMgr
    {
        private readonly GameAbilitySystem _system;

        public GameUnitInstanceMgr(GameAbilitySystem system)
        {
            _system = system;
        }

        #region Game Unit Instance Create/Destroy

        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = _system.ObjectPoolMgr.CreateObject<GameUnit>();
            unit.Init(ref param);
            NodeSystemLogger.Log("Create Unit!");
            return unit;
        }

        internal void DestroyGameUnit(GameUnit unit)
        {
            NodeSystemLogger.Log("Destroy Unit!");
            _system.ObjectPoolMgr.DestroyObject(unit);
        }

        #endregion
    }
}