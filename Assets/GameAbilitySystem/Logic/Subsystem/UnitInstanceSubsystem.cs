using System.Collections.Generic;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<int, GameUnit> _unitInstanceLookUp = new();
        private int _unitInstanceCounter;
        
        public override void UnInit()
        {
            _unitInstanceLookUp.Clear();

            var units = new List<GameUnit>();
            System.GetAllGameUnits(ref units);
            foreach (var u in units)
            {
                DestroyGameUnit(u);
            }
        }

        #region Game Unit Instance Create/Destroy

        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var unit = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameUnit>();
            unit.Init(ref param, DisposeUnit);
            _unitInstanceCounter++;
            unit.InstanceID = _unitInstanceCounter;
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
            System.GameCueSubsystem.PlayUnitCreateCue(ref context);
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
            System.GameCueSubsystem.PlayUnitDestroyCue(ref context);
            unit.OnUnitDestroyed.NotifyObservers(reason);
            System.OnUnitDestroyed.NotifyObservers(new GameUnitDestroyObserve()
            {
                Reason = reason,
                Unit = unit
            });
            unit.GetRefCountDisposableComponent().MarkForDispose();
        }

        private void DisposeUnit(GameUnit unit)
        {
            System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Release(unit);
        }

        internal GameUnit GetGameUnitByInstanceID(int unitInstanceID) => _unitInstanceLookUp.GetValueOrDefault(unitInstanceID);

        #endregion
    }
}