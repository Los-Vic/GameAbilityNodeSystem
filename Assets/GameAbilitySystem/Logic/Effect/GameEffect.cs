using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
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
        public GameUnit Instigator;
        public GameEffectCfg EffectCfg;
    }
    
    //修改单位属性
    public class GameEffect:IPoolClass, IRefCountDisposableObj
    {
        public int InstanceID { get; internal set; }
        public GameUnit Owner { get; private set; }
        public GameUnit Instigator { get; private set; }
        public string EffectName { get; private set; }
        
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        private Action<GameEffect> _disposeMethod;
        public GameEffectCfg EffectCfg { get; private set; }

        private FP _modifyDiffVal;
        private FP _lifeTimeCounter;
        
        internal void Init(ref GameEffectCreateParam param, Action<GameEffect> disposeMethod)
        {
            EffectCfg = param.EffectCfg;
            EffectName = param.EffectCfg.Name;
            Instigator = param.Instigator;
            _disposeMethod = disposeMethod;
        }
        
        private void UnInit()
        {
            _lifeTimeCounter = 0;
            _modifyDiffVal = 0;
            Owner = null;
            EffectName = string.Empty;
            EffectCfg = default;
        }

        internal void OnTick()
        {
            if (!EffectCfg.UseLifetimeVal) 
                return;
            _lifeTimeCounter += Owner.Sys.DeltaTime;

            if (_lifeTimeCounter < EffectCfg.LifetimeVal) 
                return;
            
            Owner.RemoveEffect(this);
        }
        
        internal void OnAddEffect(GameUnit owner)
        {
            Owner = owner;
            GameLogger.Log($"On add effect: {EffectName} of {Owner.UnitName}");
            
            var oldAttributeVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType);
            var modifyOutputVal = GetModifyOutputVal(owner, EffectCfg.AttributeType, EffectCfg.ModifierOp, EffectCfg.ModifierVal);
            owner.Sys.AttributeInstanceSubsystem.SetAttributeVal(owner, EffectCfg.AttributeType, modifyOutputVal, this);
            _modifyDiffVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - oldAttributeVal;

            if (EffectCfg.NotInstant)
            {
                if (EffectCfg.UseLifetimeVal)
                {
                    owner.Sys.EffectInstanceSubsystem.AddToTickList(this);
                }

                if (EffectCfg.LifeWithInstigator)
                {
                    Instigator.OnUnitDestroyed.RegisterObserver(this, (EDestroyUnitReason reason)=> Owner.RemoveEffect(this));
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    owner.Sys.GameEventSubsystem.RegisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
                }
            }

            if (!string.IsNullOrEmpty(EffectCfg.CueName))
            {
                var cueContext = new PlayEffectFxCueContext()
                {
                    EffectInstanceID = InstanceID,
                    GameCueName = EffectCfg.CueName,
                    UnitInstanceID = owner.InstanceID
                };
                owner.Sys.GameCueSubsystem.PlayEffectCue(ref cueContext);
            }
          
        }

        internal void OnRemoveEffect()
        {
            GameLogger.Log($"On remove effect: {EffectName} of {Owner.UnitName}");
            
            switch (EffectCfg.RollbackPolicy)
            {
                case EModifyRollbackPolicy.ByVal:
                    var newVal = Owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - _modifyDiffVal;
                    Owner.Sys.AttributeInstanceSubsystem.SetAttributeVal(Owner, EffectCfg.AttributeType, newVal, this);
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
                    var modifierOutput =  GetModifyOutputVal(Owner, EffectCfg.AttributeType, rollbackOp, EffectCfg.ModifierVal);
                    Owner.Sys.AttributeInstanceSubsystem.SetAttributeVal(Owner, EffectCfg.AttributeType, modifierOutput, this);
                    break;
            }
            
            if (!string.IsNullOrEmpty(EffectCfg.CueName))
            {
                var cueContext = new StopEffectFxCueContext()
                {
                    EffectInstanceID = InstanceID,
                    GameCueName = EffectCfg.CueName,
                    UnitInstanceID = Owner.InstanceID
                };
                Owner.Sys.GameCueSubsystem.StopEffectCue(ref cueContext);
            }
            
            if (EffectCfg.NotInstant)
            {
                if (EffectCfg.UseLifetimeVal || EffectCfg.LifeWithInstigator)
                {
                    Owner.Sys.EffectInstanceSubsystem.RemoveFromTickList(this);
                }

                if (EffectCfg.LifeWithInstigator)
                {
                    Instigator.OnUnitDestroyed.UnRegisterObserver(this);
                }
                
                if (EffectCfg.DeadEvent != EGameEventType.None)
                {
                    Owner.Sys.GameEventSubsystem.UnregisterGameEvent(EffectCfg.DeadEvent, OnDeadEventCall);
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
            if(!GameEventSubsystem.CheckEventFilters(Owner, arg, EffectCfg.EventFilters))
                return;
            
            Owner.RemoveEffect(this);
        }
        
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
            return _refCountDisposableComponent ?? new RefCountDisposableComponent(this);
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
            GameLogger.Log($"Release Effect: {EffectName} of {Owner?.UnitName}");
            Owner?.GameEffects.Remove(this);
            _disposeMethod(this);
        }

        #endregion
      
    }
}