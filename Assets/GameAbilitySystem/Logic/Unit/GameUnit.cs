using System;
using System.Collections.Generic;
using CommonObjectPool;
using MissQ;

namespace GAS.Logic
{
    public class GameUnit: IPoolObject
    {
        internal GameAbilitySystem Sys { get;private set; }
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect> GameEffects = new();
        
        internal void Init(GameAbilitySystem sys)
        {
            Sys = sys;
        }

        private void UnInit()
        {
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

        internal void GrantAbility(uint abilityId)
        {
            var ability = Sys.AbilityInstanceMgr.CreateAbility(abilityId);
            GameAbilities.Add(ability);
            ability.OnAddAbility(this);
        }

        internal void RemoveAbility(uint abilityId)
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

        internal void RemoveAbility(GameAbility ability)
        {
            ability.OnRemoveAbility();
            GameAbilities.Remove(ability);
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