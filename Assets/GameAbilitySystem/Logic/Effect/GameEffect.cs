using System.Collections.Generic;
using GameplayCommonLibrary;
using GameplayCommonLibrary.Handler;
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
        public bool NotInstant;
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
    public class GameEffect:IPoolClass
    {
        public Handler<GameEffect> Handler { get; private set; }
        public Handler<GameUnit> Owner { get; private set; }
        public Handler<GameUnit> Instigator { get; private set; }
        public string EffectName { get; private set; }
        
        public GameEffectCfg EffectCfg { get; private set; }

        public GameAbilitySystem Sys { get; private set; }
        private FP _modifyDiffVal;
        private FP _lifeTimeCounter;
        internal bool MarkDestroy { get; set; }
        
        internal void Init(GameAbilitySystem system, ref GameEffectInitParam param)
        {
            EffectCfg = param.CreateParam.EffectCfg;
            EffectName = param.CreateParam.EffectCfg.Name;
            Instigator = param.CreateParam.Instigator;
            Handler = param.Handler;
            Sys = system;
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
            _lifeTimeCounter += Sys.DeltaTime;

            if (_lifeTimeCounter < EffectCfg.LifetimeVal) 
                return;
            
            if(Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                owner.RemoveEffect(this);
        }
        
        internal void OnAddEffect(GameUnit owner)
        {
            Owner = owner.Handler;
            GameLogger.Log($"On add effect: {EffectName} of {owner}");
            
            var oldAttributeVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType);
            var modifyOutputVal = GetModifyOutputVal(owner, EffectCfg.AttributeType, EffectCfg.ModifierOp, EffectCfg.ModifierVal);
            Sys.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, modifyOutputVal, this);
            _modifyDiffVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - oldAttributeVal;

            if (EffectCfg.NotInstant)
            {
                if (EffectCfg.UseLifetimeVal)
                {
                    Sys.EffectInstanceSubsystem.AddToTickList(this);
                }

                if (EffectCfg.LifeWithInstigator && Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Instigator, out var instigator))
                {
                    instigator.OnUnitDestroyed.RegisterObserver(this, (EDestroyUnitReason reason)=> owner.RemoveEffect(this));
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    Sys.GameEventSubsystem.RegisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
                }
            }

            if (!string.IsNullOrEmpty(EffectCfg.CueName))
            {
                var cueContext = new PlayEffectFxCueContext()
                {
                    EffectHandler = Handler,
                    GameCueName = EffectCfg.CueName,
                    UnitHandler = Owner
                };
                Sys.GameCueSubsystem.PlayEffectCue(ref cueContext);
            }
          
        }

        internal void OnRemoveEffect()
        {
            GameLogger.Log($"On remove effect: {EffectName} of {Owner}");
            Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner);
            
            switch (EffectCfg.RollbackPolicy)
            {
                case EModifyRollbackPolicy.ByVal:
                    var newVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - _modifyDiffVal;
                    Sys.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, newVal, this);
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
                    Sys.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, modifierOutput, this);
                    break;
            }
            
            if (!string.IsNullOrEmpty(EffectCfg.CueName))
            {
                var cueContext = new StopEffectFxCueContext()
                {
                    EffectHandler = Handler,
                    GameCueName = EffectCfg.CueName,
                    UnitHandler = Owner
                };
                Sys.GameCueSubsystem.StopEffectCue(ref cueContext);
            }
            
            if (EffectCfg.NotInstant)
            {
                if (EffectCfg.UseLifetimeVal || EffectCfg.LifeWithInstigator)
                {
                    Sys.EffectInstanceSubsystem.RemoveFromTickList(this);
                }

                if (EffectCfg.LifeWithInstigator && Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Instigator, out var instigator))
                {
                    instigator.OnUnitDestroyed.UnRegisterObserver(this);
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    Sys.GameEventSubsystem.UnregisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
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
            if (!Sys.UnitInstanceSubsystem.UnitHandlerRscMgr.Dereference(Owner, out var owner))
                return;
            
            if(!GameEventSubsystem.CheckEventFilters(owner, arg, EffectCfg.EventFilters))
                return;
            
            owner.RemoveEffect(this);
        }
        
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