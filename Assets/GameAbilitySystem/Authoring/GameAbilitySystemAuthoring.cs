#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using GAS.Logic;
using MissQ;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GAS.Authoring
{
    /// <summary>
    /// A simple example, only run in editor. Use AssetDatabase load assets.
    /// </summary>
    public class GameAbilitySystemAuthoring:MonoBehaviour, IAssetConfigProvider
    {
        private GameAbilitySystem _system;
        public ConfigHub configHub;
        
        //Assets
        private readonly Dictionary<uint, AbilityAsset> _abilityAssetsCache = new();
        
        //Configs
        private readonly Dictionary<uint, AbilityConfigElement> _abilityConfigMap = new();
        private readonly Dictionary<string, List<float>> _paramMap = new();
        
        //Test
        private GameUnit _testUnit;
        
        private void Start()
        {
            _system = new GameAbilitySystem(this);
            _system.InitSystem();

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

        public AbilityAsset GetAbilityAsset(uint abilityId)
        {
            if (!_abilityConfigMap.TryGetValue(abilityId, out var abilityConfig))
                return default;

            var asset = AssetDatabase.LoadAssetAtPath<AbilityAsset>(abilityConfig.abilityAssetPath);
            if (asset == null)
                return default;

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

        #region Test

        [Button("CreateTestUnit")]
        private void CreateTestUnit()
        {
            if(!Application.isPlaying)
                return;
            
            if(_testUnit != null)
                _system.DestroyGameUnit(_testUnit);
            
            //Creat a test unit
            var param = new GameUnitCreateParam()
            {
                AbilitySystem = _system
            };
            _testUnit = _system.CreateGameUnit(ref param);
            _testUnit.GrantAbility(0);
        }

        [Button("DumpObjectPool")]
        private void DumpObjectPool()
        {
            if(!Application.isPlaying)
                return;
            
            _system.DumpObjectPool();
        }
        
        #endregion
        
    }
}
#endif