using System;
using System.Collections.Generic;
using CommonObjectPool;

namespace GameAbilitySystem.Logic
{
    public class GameUnit: IPoolObject
    {
        private GameAbilitySystem _sys;
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect> GameEffects = new();
        
        internal void Init(GameAbilitySystem sys)
        {
            _sys = sys;
        }

        private void UnInit()
        {
            //Clear Attributes
            foreach (var attribute in SimpleAttributes.Values)
            {
                _sys.AttributeInstanceMgr.DestroySimpleAttribute(attribute);
            }
            SimpleAttributes.Clear();

            foreach (var attribute in CompositeAttributes.Values)
            {
                _sys.AttributeInstanceMgr.DestroyCompositeAttribute(attribute);
            }
            CompositeAttributes.Clear();
            
            //Clear Abilities
            
            //Clear Effects
        }
        
        #region Attributes

        internal void AddSimpleAttribute(ref SimpleAttributeCreateParam param)
        {
            var attribute = _sys.AttributeInstanceMgr.CreateSimpleAttribute(ref param);
            SimpleAttributes.TryAdd(attribute.Type, attribute);
        }

        internal void RemoveSimpleAttribute(ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                _sys.AttributeInstanceMgr.DestroySimpleAttribute(attribute);
            }
        }

        internal SimpleAttribute GetSimpleAttribute(ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        internal void AddCompositeAttribute(ref CompositeAttributeCreateParam param)
        {
            var attribute = _sys.AttributeInstanceMgr.CreateCompositeAttribute(ref param);
            CompositeAttributes.TryAdd(attribute.Type, attribute);
        }

        internal void RemoveCompositeAttribute(ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                _sys.AttributeInstanceMgr.DestroyCompositeAttribute(attribute);
            }
        }

        internal CompositeAttribute GetCompositeAttribute(ECompositeAttributeType type)
        {
            return CompositeAttributes.GetValueOrDefault(type);
        }

        #endregion

        #region Abilities

        internal void GrantAbility(uint abilityId)
        {
            var ability = _sys.AbilityInstanceMgr.CreateAbility(abilityId);
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