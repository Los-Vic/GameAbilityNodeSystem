using System;
using System.Collections.Generic;
using CommonObjectPool;
using GameAbilitySystem.Logic.Ability;
using GameAbilitySystem.Logic.Attribute;
using GameAbilitySystem.Logic.Effect;
using NS;

namespace GameAbilitySystem.Logic.Unit
{
    public class GameUnit<T>: IPoolObject where T:IEquatable<T>, IComparable<T>
    {
        private GameAbilitySystem<T> _sys;
        
        //Attributes
        internal readonly Dictionary<GameAbilitySystemCfg.ESimpleAttributeType, SimpleAttribute<T>> SimpleAttributes = new();
        internal readonly Dictionary<GameAbilitySystemCfg.ECompositeAttributeType, CompositeAttribute<T>> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility<T>> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect<T>> GameEffects = new();
        

        public void Init(GameAbilitySystem<T> sys)
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

        public void AddSimpleAttribute(ref SimpleAttributeCreateParam<T> param)
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

        public SimpleAttribute<T> GetSimpleAttribute(GameAbilitySystemCfg.ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        public void AddCompositeAttribute(ref CompositeAttributeCreateParam<T> param)
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

        public CompositeAttribute<T> GetCompositeAttribute(GameAbilitySystemCfg.ECompositeAttributeType type)
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