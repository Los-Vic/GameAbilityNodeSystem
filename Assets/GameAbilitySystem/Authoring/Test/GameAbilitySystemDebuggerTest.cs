using System;
using System.Collections.Generic;
using GCL;
using GAS.Logic;
using Sirenix.OdinInspector;
using UnityEngine; 

namespace GAS
{
    public partial class GameAbilitySystemDebuggerTest : MonoBehaviour
    {
        
    }
    
#if UNITY_EDITOR
    
    [Serializable]
    public struct CreateUnitContext
    {
        public string name;
        public int playerIndex;
        public int mana;
        public int attackBase;
        public List<uint> abilities;
    }
    
    [Serializable]
    public struct SetAttributeValContext
    {
        public uint unitHandler;
        public ESimpleAttributeType type;
        public float newVal;
    }
    
    [Serializable]
    public struct AddAbilityContext
    {
        public uint unitHandler;
        public uint abilityID;
        public uint lv;
        public float signal1;
        public float signal2;
        public float signal3;
    }
    
    public partial class GameAbilitySystemDebuggerTest
    {
        //Need inject
        public GameAbilitySystem System;
        
        private List<GameUnit> _unitCache = new List<GameUnit>();
        
        [ShowInInspector]
        public const string Introduction = "Functions work only in editor play mode!";

        [BoxGroup("CreateUnitsByTemplate")]
        public DebugCreateUnitsTemplate template;

        [BoxGroup("CreateUnitsByTemplate")]
        [Button("Create Units By Template")]
        private void CreateUnitsByTemplate()
        {
            if (template is null || System == null)
                return;

            foreach (var ctx in template.unitContexts)
            {
                CreateUnitByContext(ctx);
            }
        }
        
        [BoxGroup("CreateUnit")]
        public CreateUnitContext createUnitContext;
        
        [BoxGroup("CreateUnit")]
        [Button("Create Unit")]
        private void CreateUnit()
        {
            if(System == null)
                return;

            CreateUnitByContext(createUnitContext);
        }

        private void CreateUnitByContext(CreateUnitContext context)
        {
            var param = new GameUnitCreateParam()
            {
                UnitName = context.name,
                PlayerIndex = createUnitContext.playerIndex
            };
            var unit = System.CreateGameUnit(ref param);
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.Mana,
                DefaultVal = context.mana
            });
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.AttackBase,
                DefaultVal = context.attackBase
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

            foreach (var ability in context.abilities)
            {
                unit.AddAbility(new AbilityCreateParam()
                {
                    Id = ability,
                    Lv = 1,
                    Instigator = unit.Handler,
                });
            }
        }

        [BoxGroup("SetAttributeVal")]
        public SetAttributeValContext  setAttributeValContext;

        [BoxGroup("SetAttributeVal")]
        [Button("Set Attribute Val")]
        public void SetAttributeVal()
        {
            if (System == null)
                return;
            if (!Singleton<HandlerMgr<GameUnit>>.Instance.DeRef(setAttributeValContext.unitHandler, out var u))
                return;
            System.AttributeInstanceSubsystem.SetAttributeVal(u, setAttributeValContext.type,
                setAttributeValContext.newVal);
        }
        
        [BoxGroup("AddAbility")]
        public AddAbilityContext addAbilityContext;

        [BoxGroup("AddAbility")]
        [Button("Add Ability")]
        public void AddAbility()
        {
            if (System == null)
                return;
            if (!Singleton<HandlerMgr<GameUnit>>.Instance.DeRef(addAbilityContext.unitHandler, out var u))
                return;
            
            u.AddAbility(new AbilityCreateParam()
            {
                Id = addAbilityContext.abilityID,
                Lv = addAbilityContext.lv,
                Instigator = u.Handler,
                SignalVal1 = addAbilityContext.signal1,
                SignalVal2 = addAbilityContext.signal2,
                SignalVal3 = addAbilityContext.signal3,
            });
        }
        
        [BoxGroup("PostEvent")]
        public EGameEventType gameEventType;

        [BoxGroup("PostEvent")]
        [Button("Post Event")]
        public void PostEvent()
        {
            System?.PostGameEvent(new GameEventCreateParam()
            {
                EventType = gameEventType,
            });
        }
        
        [Button("DumpObjectPool")]
        private void DumpObjectPool()
        {
            System?.DumpObjectPool();
        }
    }
    
#endif
}