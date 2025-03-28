﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using GAS.Logic;
using MissQ;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GAS
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
            _system = new GameAbilitySystem(new GameAbilitySystemCreateParam()
            {
                PlayerNums = 1,
                AssetConfigProvider = this
            });
            _system.OnCreateSystem();
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

        private void OnDestroy()
        {
            _system.UnInitSystem();
            _system.OnDestroySystem();
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

            asset.Id = abilityId;
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
                AbilitySystem = _system,
                UnitName = "TestUnit",
                PlayerIndex = 0
            };
            _testUnit = _system.CreateGameUnit(ref param);
            _testUnit.AddSimpleAttribute(new SimpleAttributeCreateParam()
            {
                Type = ESimpleAttributeType.MinionMana,
            });
            
            _testUnit.AddAbility(new AbilityCreateParam()
            {
                Id = 0,
                Lv = 1,
            });
            
            _system.PostGameEvent(new GameEventInitParam()
            {
                EventType = EGameEventType.OnPostPrepareStart,
                EventSrcUnit = _testUnit,
            });
        }

        [Button("DumpObjectPool")]
        private void DumpObjectPool()
        {
            if(!Application.isPlaying)
                return;
            
            _system.DumpObjectPool();
        }

        [Button("DumpRefCounterObjects")]
        private void DumpRefCounterObjects()
        {
            if(!Application.isPlaying)
                return;
            _system.DumpRefCounterObjects();
        }
        
        #endregion
        
    }
}
#endif