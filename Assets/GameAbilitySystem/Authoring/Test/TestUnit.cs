using Gameplay.Common;
using GAS.Logic;

namespace GAS
{
    public class TestUnit
    {
        public Handler<GameUnit> UnitHandler { get; private set; }
        public GameAbilitySystem GameAbilitySystem { get; private set; }

        public void Init(GameAbilitySystem system, Handler<GameUnit> unitHandler)
        {
            GameAbilitySystem = system;
            UnitHandler = unitHandler;

            GameAbilitySystem.HandlerManagers.UnitHandlerMgr.DeRef(unitHandler, out var unit);
            var mana = unit.GetSimpleAttribute(ESimpleAttributeType.Mana);
            GameAbilitySystem.RegisterAttributeOnPlayValChangeCue(this, mana, OnManaChangeForCue);
        }

        private void OnManaChangeForCue(AttributeChangeForCue data)
        {
            GameLogger.Log($"OnManaChangeForCue : {data.OldVal} -> {data.NewVal}");
        }
    }
}