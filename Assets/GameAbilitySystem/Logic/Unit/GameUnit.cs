using System.Collections.Generic;
using GCL;
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
    public class GameUnit: GameAbilitySystemObject, ITagOwner
    {
        private const string DefaultUnitName = "UnkownUnit";
        public Handler<GameUnit> Handler { get; private set; }
        public int PlayerIndex { get; private set; }

        public string UnitName { get; private set; } = DefaultUnitName;

        //internal readonly UnitGameCue Cue = new();
        internal ECreateUnitReason CreateReason { get; private set; }
        internal EDestroyUnitReason DestroyReason { get; private set; }
        
        //Attributes
        internal readonly Dictionary<ESimpleAttributeType, SimpleAttribute> SimpleAttributes = new();
        internal readonly Dictionary<ECompositeAttributeType, CompositeAttribute> CompositeAttributes = new();

        //Abilities
        public IReadOnlyList<Handler<GameAbility>> GameAbilities => _gameAbilities;
        private readonly List<Handler<GameAbility>> _gameAbilities = new();
        
        //Effects
        public IReadOnlyList<Handler<GameEffect>> GameEffects => _gameEffects;
        private readonly List<Handler<GameEffect>> _gameEffects = new();
        
        //Tag
        private TagContainerComponent _tagContainerComponent;
        
        //Status
        public EUnitStatus Status { get; private set; }
        
        //Logic hooks
        internal readonly Observable<EDestroyUnitReason> OnUnitDestroyed = new();
        internal readonly Observable<EGameTag> OnAddTag = new Observable<EGameTag>();
        internal readonly Observable<EGameTag> OnRemoveTag = new Observable<EGameTag>();
        
        internal void Init(ref GameUnitInitParam param)
        {
            PlayerIndex = param.CreateParam.PlayerIndex;
            UnitName = param.CreateParam.UnitName;
            CreateReason = param.CreateParam.Reason;
            Handler = param.Handler;
            Status = EUnitStatus.Normal;
            System.AbilityActivationReqSubsystem.CreateGameUnitQueue(this);
            //Cue.Init(System, Handler);
        }

        private void UnInit()
        {
            OnUnitDestroyed.Clear();
            System.AbilityActivationReqSubsystem.RemoveGameUnitQueue(this);
            UnitName = DefaultUnitName;
            //Clear Attributes
            foreach (var attribute in SimpleAttributes.Values)
            {
                System.AttributeInstanceSubsystem.DestroySimpleAttribute(attribute);
            }
            SimpleAttributes.Clear();

            foreach (var attribute in CompositeAttributes.Values)
            {
                System.AttributeInstanceSubsystem.DestroyCompositeAttribute(attribute);
            }
            CompositeAttributes.Clear();

            //Clear Abilities
            for (var i = _gameAbilities.Count - 1; i >= 0; i--)
            {
                var abilityHandler = _gameAbilities[i];
                if (!System.HandlerManagers.AbilityHandlerMgr.DeRef(abilityHandler, out var ability)) 
                    continue;
                ability.OnRemoveAbility();
                System.AbilityInstanceSubsystem.DestroyAbility(ability);
            }
            _gameAbilities.Clear();

            //Clear Effects
            for (var i = _gameEffects.Count - 1; i > 0; i--)
            {
                var effectHandler = _gameEffects[i];
                if (!System.HandlerManagers.EffectHandlerMgr.DeRef(effectHandler, out var effect)) 
                    continue;
                effect.OnRemoveEffect();
                System.EffectInstanceSubsystem.DestroyEffect(effect);
            }
            _gameEffects.Clear();
            
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
            return UnitName;
        }

        #region Attributes

        public void AddSimpleAttribute(SimpleAttributeCreateParam param)
        {
            var attribute = System.AttributeInstanceSubsystem.CreateSimpleAttribute(ref param);
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
                System.GameCueSubsystem.PlayAttributeValChangeCue(ref playCueContext);
            }, 1);
        }

        public void RemoveSimpleAttribute(ESimpleAttributeType type)
        {
            if (SimpleAttributes.Remove(type, out var attribute))
            {
                System.AttributeInstanceSubsystem.DestroySimpleAttribute(attribute);
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
            var attribute = System.AttributeInstanceSubsystem.CreateCompositeAttribute(ref param);
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
                System.GameCueSubsystem.PlayAttributeValChangeCue(ref playCueContext);
            }, 1);
        }

        public void RemoveCompositeAttribute(ECompositeAttributeType type)
        {
            if (CompositeAttributes.Remove(type, out var attribute))
            {
                System.AttributeInstanceSubsystem.DestroyCompositeAttribute(attribute);
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
            var ability = System.AbilityInstanceSubsystem.CreateAbility(ref param);
            if (ability == null)
                return;
            _gameAbilities.Add(ability.Handler);
            ability.OnAddAbility(this);
        }

        public void RemoveAbility(uint abilityId)
        {
            foreach (var abilityHandler in _gameAbilities)
            {
                if(!System.HandlerManagers.AbilityHandlerMgr.DeRef(abilityHandler, out var ability))
                    continue;
                if (abilityId != ability.ID) 
                    continue;
                RemoveAbility(ability);
            }
        }

        public void RemoveAbility(GameAbility ability)
        {
            _gameAbilities.Remove(ability.Handler);
            ability.OnRemoveAbility();
            System.AbilityInstanceSubsystem.DestroyAbility(ability);
        }

        #endregion

        #region Effects

        public void AddEffect(GameEffect effect)
        {
            _gameEffects.Add(effect.Handler);
            effect.OnAddEffect(this);
        }

        public void RemoveEffect(GameEffect effect)
        {
            _gameEffects.Remove(effect.Handler);
            effect.OnRemoveEffect();
            System.EffectInstanceSubsystem.DestroyEffect(effect);
        }

        public void RemoveEffectByName(string effectName)
        {
            var pendingRemoveEffects = new List<GameEffect>();
            foreach (var handler in _gameEffects)
            {
                if(!System.HandlerManagers.EffectHandlerMgr.DeRef(handler, out var effect))
                    continue;
                if(effect.EffectName != effectName)
                    continue;
                pendingRemoveEffects.Add(effect);
            }

            foreach (var e in pendingRemoveEffects)
            {
                RemoveEffect(e);
            }
        }

        #endregion

        #region Object Pool
        public override void OnReturnToPool()
        {
            UnInit();
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
            System.GameTagSubsystem.AddGameTag(this, t);
        }

        public void RemoveTag(EGameTag t)
        {
            if (!GetTagContainer().HasTag(t))
            {
                GameLogger.Log($"{UnitName} not has tag {t}");
                return;
            }
            System.GameTagSubsystem.RemoveGameTag(this, t);
        }
        
        #endregion
    }
}