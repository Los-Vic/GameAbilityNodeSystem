using System;
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
        public ECreateUnitReason Reason;
    }

    public enum ECreateUnitReason
    {
        None = 0,
    }
    
    public enum EDestroyUnitReason
    {
        None = 0,
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
    public class GameUnit: IPoolClass, IRefCountDisposableObj, ITagOwner
    {
        public int InstanceID { get; internal set; }
        
        internal GameAbilitySystem Sys { get;private set; }
        public int PlayerIndex { get; private set; }

        private const string DefaultUnitName = "UnkownUnit";
        private string _unitName = DefaultUnitName;
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        
        public string UnitName => _unitName;
        
        internal ECreateUnitReason CreateReason { get; set; }
        internal EDestroyUnitReason DestroyReason { get; set; }

        internal readonly Observable<EDestroyUnitReason> OnUnitDestroyed = new();
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        public readonly List<GameAbility> GameAbilities = new();
        
        //Effects
        public readonly List<GameEffect> GameEffects = new();
        
        //Tag
        private TagContainerComponent _tagContainerComponent;
        public readonly Observable<EGameTag> OnAddTag = new Observable<EGameTag>();
        public readonly Observable<EGameTag> OnRemoveTag = new Observable<EGameTag>();
        
        private AbilityActivationReqSubsystem _abilityActivationReqSubsystem;
        private Action<GameUnit> _disposeMethod;
 
        
        internal void Init(ref GameUnitCreateParam param, Action<GameUnit> disposeMethod)
        {
            Sys = param.AbilitySystem;
            PlayerIndex = param.PlayerIndex;
            _unitName = param.UnitName;
            _disposeMethod = disposeMethod;
            Sys.AbilityActivationReqSubsystem.CreateGameUnitQueue(this);
        }

        private void UnInit()
        {
            OnUnitDestroyed.Clear();
            _abilityActivationReqSubsystem.RemoveGameUnitQueue(this);
            _unitName = DefaultUnitName;
            //Clear Attributes
            foreach (var attribute in SimpleAttributes.Values)
            {
                Sys.AttributeInstanceSubsystem.DestroySimpleAttribute(attribute);
            }
            SimpleAttributes.Clear();

            foreach (var attribute in CompositeAttributes.Values)
            {
                Sys.AttributeInstanceSubsystem.DestroyCompositeAttribute(attribute);
            }
            CompositeAttributes.Clear();
            
            //Clear Abilities
            foreach (var ability in GameAbilities)
            {
                ability.OnRemoveAbility();
                Sys.AbilityInstanceSubsystem.DestroyAbility(ability);
            }
            GameAbilities.Clear();
            //Clear Effects
            foreach (var effect in GameEffects)
            {
                effect.OnRemoveEffect();
                Sys.EffectInstanceSubsystem.DestroyEffect(effect);
            }
            GameEffects.Clear();
        }

        public override string ToString()
        {
            return _unitName;
        }

        #region Attributes

        public void AddSimpleAttribute(SimpleAttributeCreateParam param)
        {
            var attribute = Sys.AttributeInstanceSubsystem.CreateSimpleAttribute(ref param);
            SimpleAttributes.TryAdd(attribute.Type, attribute);
            attribute.OnValChanged.RegisterObserver(attribute, (msg) =>
            {
                var playCueContext = new PlayAttributeValChangeCueContext()
                {
                    AttributeType = attribute.Type,
                    UnitInstanceID = InstanceID,
                    OldVal = msg.OldVal,
                    NewVal = msg.NewVal,
                };
                Sys.GameCueSubsystem.PlayAttributeValChangeCue(ref playCueContext);
            }, 1);
        }

        public void RemoveSimpleAttribute(ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                Sys.AttributeInstanceSubsystem.DestroySimpleAttribute(attribute);
            }
        }

        public SimpleAttribute GetSimpleAttribute(ESimpleAttributeType type)
        {
            return SimpleAttributes.GetValueOrDefault(type);
        }

        public void GetAllSimpleAttributes(ref List<SimpleAttribute> attributes)
        {
            attributes.Clear();
            foreach (var attribute in SimpleAttributes.Values)
            {
                attributes.Add(attribute);
            }
        }

        public FP GetSimpleAttributeVal(ESimpleAttributeType type)
        {
            var attribute = GetSimpleAttribute(type);
            return attribute?.Val ?? 0;
        }

        public void AddCompositeAttribute(CompositeAttributeCreateParam param)
        {
            var attribute = Sys.AttributeInstanceSubsystem.CreateCompositeAttribute(ref param);
            CompositeAttributes.TryAdd(attribute.Type, attribute);
            
            attribute.OnValChanged.RegisterObserver(attribute, (msg) =>
            {
                var playCueContext = new PlayAttributeValChangeCueContext()
                {
                    CompositeAttributeType = attribute.Type,
                    UnitInstanceID = InstanceID,
                    OldVal = msg.OldVal,
                    NewVal = msg.NewVal,
                };
                Sys.GameCueSubsystem.PlayAttributeValChangeCue(ref playCueContext);
            }, 1);
        }

        public void RemoveCompositeAttribute(ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                Sys.AttributeInstanceSubsystem.DestroyCompositeAttribute(attribute);
            }
        }

        public CompositeAttribute GetCompositeAttribute(ECompositeAttributeType type)
        {
            return CompositeAttributes.GetValueOrDefault(type);
        }
        
        public void GetAllCompositeAttributes(ref List<CompositeAttribute> attributes)
        {
            attributes.Clear();
            foreach (var attribute in CompositeAttributes.Values)
            {
                attributes.Add(attribute);
            }
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
            var ability = Sys.AbilityInstanceSubsystem.CreateAbility(ref param);
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
            Sys.AbilityInstanceSubsystem.DestroyAbility(ability);
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
            Sys.EffectInstanceSubsystem.DestroyEffect(effect);
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
        public void OnCreateFromPool()
        {
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
            _disposeMethod(this);
        }

        #endregion

        #region Tag

        public TagContainerComponent GetTagContainer()
        {
            return _tagContainerComponent ??= new TagContainerComponent(this);
        }
        
        public void AddTag(EGameTag t)
        {
            if (GetTagContainer().HasTag(t))
            {
                GameLogger.Log($"{UnitName} already has tag {t}");
                return;
            }
            Sys.GameTagSubsystem.AddGameTag(this, t);
        }

        public void RemoveTag(EGameTag t)
        {
            if (!GetTagContainer().HasTag(t))
            {
                GameLogger.Log($"{UnitName} not has tag {t}");
                return;
            }
            Sys.GameTagSubsystem.RemoveGameTag(this, t);
        }
        
        #endregion
    }
}