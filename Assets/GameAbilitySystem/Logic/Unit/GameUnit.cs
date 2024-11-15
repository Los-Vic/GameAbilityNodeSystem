using System;
using System.Collections.Generic;
using CommonObjectPool;

namespace GameAbilitySystem.Logic
{
    public class GameUnit: IPoolObject
    {
        private GameAbilitySystem _sys;
        
        //Attributes
        internal readonly Dictionary<GameAbilitySystemCfg.ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<GameAbilitySystemCfg.ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect> GameEffects = new();
        
        public void Init(GameAbilitySystem sys)
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

        public void AddSimpleAttribute(ref SimpleAttributeCreateParam param)
        {
            var attribute = _sys.AttributeInstanceMgr.CreateSimpleAttribute(ref param);
            SimpleAttributes.TryAdd(attribute.Type, attribute);
        }

        public void RemoveSimpleAttribute(GameAbilitySystemCfg.ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                _sys.AttributeInstanceMgr.DestroySimpleAttribute(attribute);
            }
        }

        public SimpleAttribute GetSimpleAttribute(GameAbilitySystemCfg.ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        public void AddCompositeAttribute(ref CompositeAttributeCreateParam param)
        {
            var attribute = _sys.AttributeInstanceMgr.CreateCompositeAttribute(ref param);
            CompositeAttributes.TryAdd(attribute.Type, attribute);
        }

        public void RemoveCompositeAttribute(GameAbilitySystemCfg.ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                _sys.AttributeInstanceMgr.DestroyCompositeAttribute(attribute);
            }
        }

        public CompositeAttribute GetCompositeAttribute(GameAbilitySystemCfg.ECompositeAttributeType type)
        {
            return CompositeAttributes.GetValueOrDefault(type);
        }

        #endregion

        #region Abilities

        public void GrantAbility()
        {
            
        }

        public void RemoveAbility()
        {
            
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