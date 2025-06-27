using System;
using System.Collections.Generic;
using GAS.Logic;
using GAS.Logic.Target;
using MissQ;
using UnityEditor;
using UnityEngine;

namespace GAS
{
    public partial class GameAbilitySystemAuthoring:MonoBehaviour
    {
        
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// A simple example, only run in editor. Use AssetDatabase load assets.
    /// </summary>
    public partial class GameAbilitySystemAuthoring:IAssetConfigProvider, ICommandDelegator, ITargetSearcher
    {
        private GameAbilitySystem _system;
        private GameAbilitySystemDebugger _debugger;
        
        public ConfigHub configHub;
        
        //Assets
        private readonly Dictionary<uint, AbilityAsset> _abilityAssetsCache = new();
        
        //Configs
        private readonly Dictionary<uint, AbilityConfigElement> _abilityConfigMap = new();
        private readonly Dictionary<string, List<float>> _paramMap = new();

        private void Awake()
        {
            _debugger = GetComponent<GameAbilitySystemDebugger>();
        }

        private void Start()
        {
            _system = new GameAbilitySystem(new GameAbilitySystemCreateParam()
            {
                PlayerNums = 1,
                AssetConfigProvider = this,
                CommandDelegator = this,
                TargetSearcher = this,
            });
            _system.OnCreateSystem();
            _system.InitSystem();
            _debugger.System = _system;
            
            if (configHub.abilityEffectParamConfig)
            {
                foreach (var element in configHub.abilityEffectParamConfig.paramElements)
                {
                    _paramMap.TryAdd(element.paramName, element.paramVals);
                }
            }

            if (configHub.abilityConfig)
            {
                foreach (var element in configHub.abilityConfig.elements)
                {
                    _abilityConfigMap.TryAdd(element.id, element);
                }
            }
        }

        private void Update()
        {
            _system.UpdateSystem(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _system.UnInitSystem();
        }

        public AbilityAsset GetAbilityAsset(uint abilityId)
        {
            if(_abilityAssetsCache.TryGetValue(abilityId, out var abilityAsset))
                return abilityAsset;
            
            if (!_abilityConfigMap.TryGetValue(abilityId, out var abilityConfig))
                return null;

            var asset = AssetDatabase.LoadAssetAtPath<AbilityAsset>(abilityConfig.abilityAssetPath);
            if (asset == null)
                return null;
            
            _abilityAssetsCache.TryAdd(abilityId, asset);
            return asset;
        }

        public FP GetAbilityEffectParamVal(string paramName, uint lv)
        {
            if (!_paramMap.TryGetValue(paramName, out var paramVals))
                return 0;

            if (paramVals.Count == 0)
                return 0;
            
            var id = (int)Math.Clamp(lv, 0, paramVals.Count - 1);
            return paramVals[id];
        }

        public GameUnit SpawnUnit(string unitName, int playerIndex)
        {
            var param = new GameUnitCreateParam()
            {
                AbilitySystem = _system,
                UnitName = unitName,
                PlayerIndex = playerIndex
            };
            var unit = _system.CreateGameUnit(ref param);
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.Mana,
            });
            unit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.AttackBase,
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
            return unit;
        }

        public bool GetTargetFromAbility(GameAbility ability, TargetQuerySingleBase cfg, out GameUnit target, bool ignoreSelf = false)
        {
            target = null;
            return false;
        }

        public bool GetTargetsFromAbility(GameAbility ability, TargetQueryMultipleBase cfg, ref List<GameUnit> targets, bool ignoreSelf = false)
        {
            targets.Clear();
            switch (cfg)
            {
                case TargetQueryGameTagMultiple t:
                    TargetQueryGameTagUtility.GetQueryResult(ability, t, ref targets, ignoreSelf);
                    break;
            }
            return false;
        }
    }
}
#endif