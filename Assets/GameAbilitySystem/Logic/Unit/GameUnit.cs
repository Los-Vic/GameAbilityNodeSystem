using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public struct GameUnitCreateParam
    {
        public GameAbilitySystem AbilitySystem;
        public string UnitName;
        public int PlayerIndex;
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
    public class GameUnit: IPoolClass, IRefCountDisposableObj
    {
        internal GameAbilitySystem Sys { get;private set; }
        public int PlayerIndex { get; private set; }

        private const string DefaultUnitName = "UnkownUnit";
        private string _unitName = DefaultUnitName;
        private ClassObjectPool _pool;
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        
        internal string UnitName => _unitName;
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        internal readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        internal readonly List<GameEffect> GameEffects = new();
        
        internal void Init(ref GameUnitCreateParam param)
        {
            Sys = param.AbilitySystem;
            PlayerIndex = param.PlayerIndex;
            _unitName = param.UnitName;
            
            Sys.GetSubsystem<AbilityActivationReqSubsystem>().CreateGameUnitQueue(this);
        }

        private void UnInit()
        {
            Sys.GetSubsystem<AbilityActivationReqSubsystem>().RemoveGameUnitQueue(this);
            _unitName = DefaultUnitName;
            //Clear Attributes
            foreach (var attribute in SimpleAttributes.Values)
            {
                Sys.GetSubsystem<AttributeInstanceSubsystem>().DestroySimpleAttribute(attribute);
            }
            SimpleAttributes.Clear();

            foreach (var attribute in CompositeAttributes.Values)
            {
                Sys.GetSubsystem<AttributeInstanceSubsystem>().DestroyCompositeAttribute(attribute);
            }
            CompositeAttributes.Clear();
            
            //Clear Abilities
            foreach (var ability in GameAbilities)
            {
                ability.OnRemoveAbility();
                Sys.GetSubsystem<AbilityInstanceSubsystem>().DestroyAbility(ability);
            }
            GameAbilities.Clear();
            //Clear Effects
            foreach (var effect in GameEffects)
            {
                effect.OnRemoveEffect();
                Sys.GetSubsystem<EffectInstanceSubsystem>().DestroyEffect(effect);
            }
            GameEffects.Clear();
        }
        
        #region Attributes

        public void AddSimpleAttribute(SimpleAttributeCreateParam param)
        {
            var attribute = Sys.GetSubsystem<AttributeInstanceSubsystem>().CreateSimpleAttribute(ref param);
            SimpleAttributes.TryAdd(attribute.Type, attribute);
        }

        public void RemoveSimpleAttribute(ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                Sys.GetSubsystem<AttributeInstanceSubsystem>().DestroySimpleAttribute(attribute);
            }
        }

        public SimpleAttribute GetSimpleAttribute(ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        public FP GetSimpleAttributeVal(ESimpleAttributeType type)
        {
            var attribute = GetSimpleAttribute(type);
            return attribute?.Val ?? 0;
        }

        public void AddCompositeAttribute(CompositeAttributeCreateParam param)
        {
            var attribute = Sys.GetSubsystem<AttributeInstanceSubsystem>().CreateCompositeAttribute(ref param);
            CompositeAttributes.TryAdd(attribute.Type, attribute);
        }

        public void RemoveCompositeAttribute(ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                Sys.GetSubsystem<AttributeInstanceSubsystem>().DestroyCompositeAttribute(attribute);
            }
        }

        public CompositeAttribute GetCompositeAttribute(ECompositeAttributeType type)
        {
            return CompositeAttributes.GetValueOrDefault(type);
        }

        public FP GetCompositeAttributeVal(ECompositeAttributeType type)
        {
            var attribute = GetCompositeAttribute(type);
            return attribute?.Val ?? 0;
        }
        
        #endregion

        #region Abilities

        public void AddAbility(AbilityCreateParam param)
        {
            var ability = Sys.GetSubsystem<AbilityInstanceSubsystem>().CreateAbility(ref param);
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
                RemoveAbility(ability);
                break;
            }
        }

        public void RemoveAbility(GameAbility ability)
        {
            ability.OnRemoveAbility();
            Sys.GetSubsystem<AbilityInstanceSubsystem>().DestroyAbility(ability);
        }

        #endregion

        #region Effects

        public void AddEffect(GameEffect effect)
        {
            GameEffects.Add(effect);
            effect.OnAddEffect(this);
        }

        public void RemoveEffect(GameEffect effect)
        {
            effect.OnRemoveEffect();
            Sys.GetSubsystem<EffectInstanceSubsystem>().DestroyEffect(effect);
        }

        public void RemoveEffectByName(string effectName)
        {
            var pendingRemoveEffects = new List<GameEffect>();
            foreach (var e in GameEffects)
            {
                if(e.EffectName != effectName)
                    continue;
                pendingRemoveEffects.Add(e);
            }

            foreach (var e in pendingRemoveEffects)
            {
                RemoveEffect(e);
            }
        }

        #endregion

        #region Object Pool
        public void OnCreateFromPool(ClassObjectPool pool)
        {
            _pool = pool;
        }

        public void OnTakeFromPool()
        {
            _isActive = true;
        }

        public void OnReturnToPool()
        {
            _isActive = false;
            UnInit();
        }

        public void OnDestroy()
        {
            
        }

        #endregion

        #region IRefCountDisposableObj

        public RefCountDisposableComponent GetRefCountDisposableComponent()
        {
            return _refCountDisposableComponent ??= new RefCountDisposableComponent(this);
        }

        public bool IsDisposed()
        {
            return !_isActive;
        }

        public void ForceDisposeObj()
        {
            GetRefCountDisposableComponent().DisposeOwner();
        }

        public void OnObjDispose()
        {
            GameLogger.Log($"Release Unit: {UnitName}");
            _pool.Release(this);
        }

        #endregion
    }
}