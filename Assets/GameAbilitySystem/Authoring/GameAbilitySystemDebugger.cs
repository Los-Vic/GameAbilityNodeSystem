using System;
using System.Collections.Generic;
using GAS.Logic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS
{
    public partial class GameAbilitySystemDebugger : MonoBehaviour
    {
        
    }
    
#if UNITY_EDITOR
    
    [Serializable]
    public struct CreateUnitContext
    {
        public string name;
        public int playerIndex;
    }
    
    [Serializable]
    public struct SetAttributeValContext
    {
        public int unitInstanceID;
        public ESimpleAttributeType type;
        public float newVal;
    }
    
    [Serializable]
    public struct AddAbilityContext
    {
        public int unitInstanceID;
        public uint abilityID;
        public uint lv;
        public float signal1;
        public float signal2;
        public float signal3;
    }
    
    public partial class GameAbilitySystemDebugger:MonoBehaviour
    {
        //Need inject
        public GameAbilitySystem System;
        
        private List<GameUnit> _unitCache = new List<GameUnit>();
        
        [ShowInInspector]
        private string _introduction = "Functions work only in editor play mode!";
        
        [BoxGroup("CreateUnit")]
        public CreateUnitContext createUnitContext;
        
        [BoxGroup("CreateUnit")]
        [Button("Create Unit")]
        private void CreateUnit()
        {
            if(System == null)
                return;

            var param = new GameUnitCreateParam()
            {
                AbilitySystem = System,
                UnitName = createUnitContext.name,
                PlayerIndex = createUnitContext.playerIndex
            };
            var unit = System.CreateGameUnit(ref param);
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.Mana,
            });
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.AttackBase,
                DefaultVal = 5
            });
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.AttackAdd
            });
            unit.AddCompositeAttribute(new CompositeAttributeCreateParam()
            {
                Type = ECompositeAttributeType.Attack,
                SimpleAttributes = new List<SimpleAttribute>()
                {
                    unit.GetSimpleAttribute(ESimpleAttributeType.AttackBase),
                    unit.GetSimpleAttribute(ESimpleAttributeType.AttackAdd)
                },
                ValEquation = valList => valList[0] + valList[1]
            });
            unit.AddTag(EGameTag.Unit);
        }

        [BoxGroup("SetAttributeVal")]
        public SetAttributeValContext  setAttributeValContext;

        [BoxGroup("SetAttributeVal")]
        [Button("Set Attribute Val")]
        public void SetAttributeVal()
        {
            System?.GetAllGameUnits(ref _unitCache);
            foreach (var u in _unitCache)
            {
                if (u.InstanceID != setAttributeValContext.unitInstanceID) 
                    continue;
                System?.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(u, setAttributeValContext.type,
                    setAttributeValContext.newVal);
                break;
            }
        }
        
        [BoxGroup("AddAbility")]
        public AddAbilityContext addAbilityContext;

        [BoxGroup("AddAbility")]
        [Button("Add Ability")]
        public void AddAbility()
        {
            System?.GetAllGameUnits(ref _unitCache);
            foreach (var u in _unitCache)
            {
                if (u.InstanceID != addAbilityContext.unitInstanceID) 
                    continue;
                u.AddAbility(new AbilityCreateParam()
                {
                    Id = addAbilityContext.abilityID,
                    Lv = addAbilityContext.lv,
                    Instigator = u,
                    SignalVal1 = addAbilityContext.signal1,
                    SignalVal2 = addAbilityContext.signal2,
                    SignalVal3 = addAbilityContext.signal3,
                });
                break;
            }
        }
        
        
        [Button("DumpObjectPool")]
        private void DumpObjectPool()
        {
            System?.DumpObjectPool();
        }

        [Button("DumpRefCounterObjects")]
        private void DumpRefCounterObjects()
        {
            System?.DumpRefCounterObjects();
        }
    }
    
#endif
}