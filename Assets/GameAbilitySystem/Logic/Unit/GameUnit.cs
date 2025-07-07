using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;
using MissQ;

namespace GAS.Logic
{
    public struct GameUnitCreateParam
    {
        public string UnitName;
        public int PlayerIndex;
        public ECreateUnitReason Reason;
    }

    public struct GameUnitInitParam
    {
        public GameUnitCreateParam CreateParam;
        public Handler<GameUnit> Handler;
    }
    
    public enum ECreateUnitReason
    {
        None = 0,
    }
    
    public enum EDestroyUnitReason
    {
        None = 0,
    }

    public enum EUnitStatus
    {
        Normal,
        PendingDestroy,
        Destroyed,
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
    public class GameUnit: IPoolClass, ITagOwner
    {
        private const string DefaultUnitName = "UnkownUnit";
        public Handler<GameUnit> Handler { get; private set; }
        public int PlayerIndex { get; private set; }
       
        private string _unitName = DefaultUnitName;
        public string UnitName => _unitName;
        
        internal ECreateUnitReason CreateReason { get; private set; }
        internal EDestroyUnitReason DestroyReason { get; private set; }

        internal readonly Observable<EDestroyUnitReason> OnUnitDestroyed = new();
        
        internal GameAbilitySystem Sys { get;private set; }
        
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
        
        //Status
        public EUnitStatus Status { get; private set; }
        
        private AbilityActivationReqSubsystem _abilityActivationReqSubsystem;
        
        internal void Init(GameAbilitySystem sys, ref GameUnitInitParam param)
        {
            Sys = sys;
            PlayerIndex = param.CreateParam.PlayerIndex;
            _unitName = param.CreateParam.UnitName;
            CreateReason = param.CreateParam.Reason;
            Handler = param.Handler;
            Status = EUnitStatus.Normal;
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
            Status = EUnitStatus.Destroyed;
            Handler = 0;
        }

        internal void MarkForDestroy(EDestroyUnitReason reason)
        {
            DestroyReason = reason;
            Status = EUnitStatus.PendingDestroy;
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
                    UnitHandler = Handler,
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
                    UnitHandler = Handler,
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
        }

        public void OnReturnToPool()
        {
            UnInit();
        }

        public void OnDestroy()
        {
            
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