using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<int, GameUnit> _unitInstanceLookUp = new();
        
        public override void UnInit()
        {
            _unitInstanceLookUp.Clear();
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
            _unitInstanceLookUp.Add(unit.InstanceID, unit);
            unit.CreateReason = param.Reason;
            System.OnUnitCreated.NotifyObservers(new GameUnitCreateObserve()
            {
                Reason = param.Reason,
                Unit = unit
            });
            var context = new UnitCreateCueContext()
            {
                UnitInstanceID = unit.InstanceID
            };
            System.GetSubsystem<GameCueSubsystem>().PlayUnitCreateCue(ref context);
            GameLogger.Log($"Create unit:{param.UnitName}, reason:{param.Reason}");
            return unit;
        }

        internal void DestroyGameUnit(GameUnit unit, EDestroyUnitReason reason = EDestroyUnitReason.None)
        {
            GameLogger.Log($"Destroy unit:{unit.UnitName}, reason:{reason}");
            _unitInstanceLookUp.Remove(unit.InstanceID);
            unit.DestroyReason = reason;
            var context = new UnitDestroyCueContext()
            {
                UnitInstanceID = unit.InstanceID
            };
            System.GetSubsystem<GameCueSubsystem>().PlayUnitDestroyCue(ref context);
            unit.OnUnitDestroyed.NotifyObservers(reason);
            System.OnUnitDestroyed.NotifyObservers(new GameUnitDestroyObserve()
            {
                Reason = reason,
                Unit = unit
            });
            unit.GetRefCountDisposableComponent().MarkForDispose();
        }

        internal GameUnit GetGameUnitByInstanceID(int unitInstanceID) => _unitInstanceLookUp.GetValueOrDefault(unitInstanceID);

        #endregion
    }
}