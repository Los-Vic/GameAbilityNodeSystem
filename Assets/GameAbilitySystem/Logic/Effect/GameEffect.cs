using System.Collections.Generic;
using Gameplay.Common;
using MissQ;
using UnityEngine;

namespace GAS.Logic
{
    public enum EModifierOp
    {
        [InspectorName("无")]
        None,
        [InspectorName("加")]
        Add,
        [InspectorName("减")]
        Subtract,
        [InspectorName("乘")]
        Multiply,
        [InspectorName("除")]
        Divide,
        [InspectorName("覆盖")]
        Override
    }

    public enum EModifyRollbackPolicy
    {
        [InspectorName("无需回退")]
        None,
        [InspectorName("回退变化的差值")]
        ByVal,
        [InspectorName("使用公式回退")]
        ByOp
    }
    
    public struct GameEffectCfg
    {
        public string Name;
        public ESimpleAttributeType AttributeType;
        public EModifierOp ModifierOp;
        public FP ModifierVal;
        public EModifyRollbackPolicy RollbackPolicy;
        public bool IsPersistent;
        public FP LifetimeVal;
        public bool UseLifetimeVal;
        public bool LifeWithInstigator;
        public EGameEventType DeadEvent;
        public List<EGameEventFilter> EventFilters;
        public string CueName;
    }
    
    public struct GameEffectCreateParam
    {
        public Handler<GameUnit> Instigator;
        public GameEffectCfg EffectCfg;
    }

    public struct GameEffectInitParam
    {
        public GameEffectCreateParam CreateParam;
        public Handler<GameEffect> Handler;
    }
    
    //修改单位属性
    public class GameEffect:GameAbilitySystemObject
    {
        public Handler<GameEffect> Handler { get; private set; }
        public Handler<GameUnit> Owner { get; private set; }
        public Handler<GameUnit> Instigator { get; private set; }
        public string EffectName { get; private set; }
        
        public GameEffectCfg EffectCfg { get; private set; }
        
        private FP _modifyDiffVal;
        private FP _lifeTimeCounter;
        internal bool MarkDestroy { get; set; }
        internal readonly EffectGameCue Cue = new();
        
        internal void Init(ref GameEffectInitParam param)
        {
            EffectCfg = param.CreateParam.EffectCfg;
            Instigator = param.CreateParam.Instigator;
            Handler = param.Handler;
            Cue.Init(System, Handler);
        }
        
        private void UnInit()
        {
            _lifeTimeCounter = 0;
            _modifyDiffVal = 0;
            Owner = 0;
            Instigator = 0;
            EffectName = string.Empty;
            EffectCfg = default;
            Handler = 0;
            MarkDestroy = false;
        }

        public override string ToString()
        {
            return EffectName;
        }

        internal void OnTick()
        {
            if (!EffectCfg.UseLifetimeVal) 
                return;
            _lifeTimeCounter += System.DeltaTime;

            if (_lifeTimeCounter < EffectCfg.LifetimeVal) 
                return;
            
            if(System.HandlerManagers.UnitHandlerMgr.DeRef(Owner, out var owner))
                owner.RemoveEffect(this);
        }
        
        internal void OnAddEffect(GameUnit owner)
        {
            Owner = owner.Handler;
            EffectName = $"{EffectCfg.Name} of {owner}";
            GameLogger.Log($"On add effect: {EffectName}");
            
            var oldAttributeVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType);
            var modifyOutputVal = GetModifyOutputVal(owner, EffectCfg.AttributeType, EffectCfg.ModifierOp, EffectCfg.ModifierVal);
            System.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, modifyOutputVal, this);
            _modifyDiffVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - oldAttributeVal;

            if (!string.IsNullOrEmpty(EffectCfg.CueName))
            {
                var cueContext = new PlayEffectFxCueContext()
                {
                    EffectHandler = Handler,
                    GameCueName = EffectCfg.CueName,
                    UnitHandler = Owner,
                    IsPersistent = EffectCfg.IsPersistent
                };
                Cue.PlayEffectCue(ref cueContext);
            }
            
            if (EffectCfg.IsPersistent)
            {
                if (EffectCfg.UseLifetimeVal)
                {
                    System.EffectInstanceSubsystem.AddToTickList(this);
                }

                if (EffectCfg.LifeWithInstigator && System.HandlerManagers.UnitHandlerMgr.DeRef(Instigator, out var instigator))
                {
                    instigator.OnUnitDestroyed.RegisterObserver(this, (EDestroyUnitReason reason)=> owner.RemoveEffect(this));
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    System.GameEventSubsystem.RegisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
                }
            }
            else
            {
                owner.RemoveEffect(this);
            }
        }

        internal void OnRemoveEffect()
        {
            GameLogger.Log($"On remove effect: {EffectName}");
            System.HandlerManagers.UnitHandlerMgr.DeRef(Owner, out var owner);
            
            switch (EffectCfg.RollbackPolicy)
            {
                case EModifyRollbackPolicy.ByVal:
                    var newVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - _modifyDiffVal;
                    System.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, newVal, this);
                    break;
                case EModifyRollbackPolicy.ByOp:
                    var rollbackOp = EffectCfg.ModifierOp switch
                    {
                        EModifierOp.Add => EModifierOp.Subtract,
                        EModifierOp.Subtract => EModifierOp.Add,
                        EModifierOp.Multiply => EModifierOp.Divide,
                        EModifierOp.Divide => EModifierOp.Multiply,
                        _ => EModifierOp.None
                    };
                    var modifierOutput =  GetModifyOutputVal(owner, EffectCfg.AttributeType, rollbackOp, EffectCfg.ModifierVal);
                    System.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, modifierOutput, this);
                    break;
            }
            
            if (EffectCfg.IsPersistent)
            {
                if (!string.IsNullOrEmpty(EffectCfg.CueName))
                {
                    var cueContext = new StopEffectFxCueContext()
                    {
                        EffectHandler = Handler,
                        GameCueName = EffectCfg.CueName,
                        UnitHandler = Owner
                    };
                    Cue.StopEffectCue(ref cueContext);
                }
                
                if (EffectCfg.UseLifetimeVal || EffectCfg.LifeWithInstigator)
                {
                    System.EffectInstanceSubsystem.RemoveFromTickList(this);
                }

                if (EffectCfg.LifeWithInstigator && System.HandlerManagers.UnitHandlerMgr.DeRef(Instigator, out var instigator))
                {
                    instigator.OnUnitDestroyed.UnRegisterObserver(this);
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    System.GameEventSubsystem.UnregisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
                }
            }
        }
        
        internal static FP GetModifyOutputVal(GameUnit unit, ESimpleAttributeType attributeType, EModifierOp op, FP modifierVal)
        {
            var attributeVal = unit.GetSimpleAttributeVal(attributeType);
            switch (op)
            {
                case EModifierOp.Add:
                    return attributeVal + modifierVal;
                case EModifierOp.Subtract:
                    return attributeVal - modifierVal;
                case EModifierOp.Multiply:
                    return attributeVal * modifierVal;
                case EModifierOp.Divide:
                    return attributeVal / modifierVal;
                case EModifierOp.Override:
                    return modifierVal;
            }
            return attributeVal;
        }
        
        private void OnDeadEventCall(GameEventArg arg)
        {
            if (!System.HandlerManagers.UnitHandlerMgr.DeRef(Owner, out var owner))
                return;
            
            if(!GameEventSubsystem.CheckEventFilters(owner, arg, EffectCfg.EventFilters))
                return;
            
            owner.RemoveEffect(this);
        }
        
        #region Object Pool

        public override void OnReturnToPool()
        {
            UnInit();
        }

        #endregion
    }
}