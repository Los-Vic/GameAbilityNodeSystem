using System.Collections.Generic;
using System.Linq;
using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class UnitInstanceSubsystem:GameAbilitySubsystem
    {
        private readonly Dictionary<int, GameUnit> _unitInstanceLookUp = new();
        private int _unitInstanceIDCounter;
        
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

        public void GetAllUnits(ref List<GameUnit> unitList)
        {
            unitList.Clear();
            unitList.AddRange(_unitInstanceLookUp.Values);
        }
        
        private int GetNextUnitInstanceID()
        {
            while (true)
            {
                var nextId = _unitInstanceIDCounter + 1;
                if (_unitInstanceLookUp.ContainsKey(nextId)) 
                    continue;
                _unitInstanceIDCounter = nextId;
                return nextId;
            }
        }

        #region Game Unit Instance Create/Destroy

        internal GameUnit CreateGameUnit(ref GameUnitCreateParam param)
        {
            var paramEx = new GameUnitCreateParamEx()
            {
                BaseParam = param,
                UnitInstanceID = GetNextUnitInstanceID()
            };

            return CreateGameUnitEx(ref paramEx);
        }

        internal GameUnit CreateGameUnitEx(ref GameUnitCreateParamEx paramEx)
        {
            if (_unitInstanceLookUp.ContainsKey(paramEx.UnitInstanceID))
            {
                GameLogger.LogError($"Create game unit failed. unit instance id has existed ! {paramEx.UnitInstanceID}");
                return null;
            }
            
            var unit = System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Get<GameUnit>();
            unit.Init(System, ref paramEx, DisposeUnit);
            _unitInstanceLookUp.Add(unit.InstanceID, unit);
            System.OnUnitCreated.NotifyObservers(new GameUnitCreateObserve()
            {
                Reason = paramEx.BaseParam.Reason,
                Unit = unit
            });
            var context = new UnitCreateCueContext()
            {
                UnitInstanceID = unit.InstanceID
            };
            System.GameCueSubsystem.PlayUnitCreateCue(ref context);
            GameLogger.Log($"Create unit:{paramEx.BaseParam.UnitName}, reason:{paramEx.BaseParam.Reason}");
            return unit;
        }
        
        internal void DestroyGameUnit(GameUnit unit, EDestroyUnitReason reason = EDestroyUnitReason.None)
        {
            GameLogger.Log($"Destroy unit:{unit}, reason:{reason}");
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

        internal void DestroyGameUnit(int unitInstanceID, EDestroyUnitReason reason = EDestroyUnitReason.None)
        {
            var unit = GetGameUnitByInstanceID(unitInstanceID);
            if (unit == null)
                return;
            DestroyGameUnit(unit, reason);
        }

        private void DisposeUnit(GameUnit unit)
        {
            System.ClassObjectPoolSubsystem.ClassObjectPoolMgr.Release(unit);
        }

        internal GameUnit GetGameUnitByInstanceID(int unitInstanceID) => _unitInstanceLookUp.GetValueOrDefault(unitInstanceID);

        #endregion
    }
}