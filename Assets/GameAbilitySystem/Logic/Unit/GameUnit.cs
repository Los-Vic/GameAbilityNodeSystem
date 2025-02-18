using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;
using NS;

namespace GAS.Logic
{
    public struct GameUnitCreateParam
    {
        public GameAbilitySystem AbilitySystem;
        public string UnitName;
    }

    public enum EUnitAbilityCastState
    {
        Idle,
        PreCasting,
        Casting,
        PostCasting,
    }
    
    /// <summary>
    /// GameUnit：
    /// 1、拥有属性
    /// 2、可以增加和移除技能
    /// 3、可以被添加效果
    /// 4、可以被技能系统作为对象选取
    /// 5、技能前后摇，技能施放队列管理
    /// 6、单位标签
    /// </summary>
    public class GameUnit: IPoolObject
    {
        internal GameAbilitySystem Sys { get;private set; }

        private const string DefaultUnitName = "UnkownUnit";
        private string _unitName = DefaultUnitName;
        internal string UnitName => _unitName;
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect> GameEffects = new();
        
        //Ability Cast
        private GameAbility _castingAbility;
        internal EUnitAbilityCastState CastingState { get; private set; }
        internal bool CanCastNewAbility => CastingState == EUnitAbilityCastState.Idle;
        
        internal void Init(ref GameUnitCreateParam param)
        {
            Sys = param.AbilitySystem;
            _unitName = param.UnitName;
        }

        private void UnInit()
        {
            _unitName = DefaultUnitName;
            //Clear Attributes
            foreach (var attribute in SimpleAttributes.Values)
            {
                Sys.AttributeInstanceMgr.DestroySimpleAttribute(attribute);
            }
            SimpleAttributes.Clear();

            foreach (var attribute in CompositeAttributes.Values)
            {
                Sys.AttributeInstanceMgr.DestroyCompositeAttribute(attribute);
            }
            CompositeAttributes.Clear();
            
            //Clear Abilities
            foreach (var ability in GameAbilities)
            {
                Sys.AbilityInstanceMgr.DestroyAbility(ability);
            }
            GameAbilities.Clear();
            //Clear Effects
        }
        
        #region Attributes

        internal void AddSimpleAttribute(ref SimpleAttributeCreateParam param)
        {
            var attribute = Sys.AttributeInstanceMgr.CreateSimpleAttribute(ref param);
            SimpleAttributes.TryAdd(attribute.Type, attribute);
        }

        internal void RemoveSimpleAttribute(ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                Sys.AttributeInstanceMgr.DestroySimpleAttribute(attribute);
            }
        }

        internal SimpleAttribute GetSimpleAttribute(ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        internal FP GetSimpleAttributeVal(ESimpleAttributeType type)
        {
            var attribute = GetSimpleAttribute(type);
            return attribute?.Val ?? 0;
        }

        internal void AddCompositeAttribute(ref CompositeAttributeCreateParam param)
        {
            var attribute = Sys.AttributeInstanceMgr.CreateCompositeAttribute(ref param);
            CompositeAttributes.TryAdd(attribute.Type, attribute);
        }

        internal void RemoveCompositeAttribute(ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                Sys.AttributeInstanceMgr.DestroyCompositeAttribute(attribute);
            }
        }

        internal CompositeAttribute GetCompositeAttribute(ECompositeAttributeType type)
        {
            return CompositeAttributes.GetValueOrDefault(type);
        }

        internal FP GetCompositeAttributeVal(ECompositeAttributeType type)
        {
            var attribute = GetCompositeAttribute(type);
            return attribute?.Val ?? 0;
        }
        
        #endregion

        #region Abilities

        public void GrantAbility(uint abilityId)
        {
            var ability = Sys.AbilityInstanceMgr.CreateAbility(abilityId);
            if (ability == null)
                return;
            GameAbilities.Add(ability);
            ability.OnAddAbility(this);
        }

        public void RemoveAbility(uint abilityId)
        {
            foreach (var ability in GameAbilities)
            {
                if (abilityId != ability.ID) 
                    continue;
                ability.OnRemoveAbility();
                GameAbilities.Remove(ability);
                break;
            }
        }

        public void RemoveAbility(GameAbility ability)
        {
            ability.OnRemoveAbility();
            GameAbilities.Remove(ability);
        }

        #endregion

        #region Cast Ability

        internal void CastAbility(GameAbility ability)
        {
            if (!CanCastNewAbility)
            {
                NodeSystemLogger.Log($"Cast ability failed, state is not idle. unit {UnitName}, ability {ability.AbilityName}");
                return;
            }
            
            
            
        }

        #endregion

        #region Object Pool
        public void OnCreateFromPool()
        {
            
        }

        public void OnTakeFromPool()
        {
        }

        public void OnReturnToPool()
        {
            UnInit();
        }

        public void OnDestroy()
        {
            
        }

        #endregion
        
    }
}