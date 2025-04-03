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
        public FP LifetimeVal;
        public bool UseLifetimeVal;
        public bool LifeWithInstigator;
        public EGameEventType DeadEvent;
    }
    
    public struct GameEffectCreateParam
    {
        public GameUnit Instigator;
        public GameEffectCfg EffectCfg;
    }
    
    //修改单位属性
    public class GameEffect:IPoolClass, IRefCountDisposableObj
    {
        public GameUnit Owner { get; private set; }
        public GameUnit Instigator { get; private set; }
        public string EffectName;
        
        private RefCountDisposableComponent _refCountDisposableComponent;
        private bool _isActive;
        private ClassObjectPool _pool;
        public GameEffectCfg EffectCfg { get; private set; }

        private FP _modifyDiffVal;
        
        internal void Init(ref GameEffectCreateParam param)
        {
            EffectCfg = param.EffectCfg;
            EffectName = param.EffectCfg.Name;
            Instigator = param.Instigator;
        }
        
        private void UnInit()
        {
            _modifyDiffVal = 0;
            Owner = null;
            EffectName = string.Empty;
        }
        
        internal void OnAddEffect(GameUnit owner)
        {
            Owner = owner;
            GameLogger.Log($"On add effect: {EffectName} of {Owner.UnitName}");
            
            var oldAttributeVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType);
            var modifyOutputVal = GetModifyOutputVal(owner, EffectCfg.AttributeType, EffectCfg.ModifierOp, EffectCfg.ModifierVal);
            owner.Sys.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(owner, EffectCfg.AttributeType, modifyOutputVal, this);
            _modifyDiffVal = owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - oldAttributeVal;
        }

        internal void OnRemoveEffect()
        {
            GameLogger.Log($"On remove effect: {EffectName} of {Owner.UnitName}");

            switch (EffectCfg.RollbackPolicy)
            {
                case EModifyRollbackPolicy.ByVal:
                    var newVal = Owner.GetSimpleAttributeVal(EffectCfg.AttributeType) - _modifyDiffVal;
                    Owner.Sys.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(Owner, EffectCfg.AttributeType, newVal, this);
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
                    Owner.Sys.GetSubsystem<AttributeInstanceSubsystem>().SetAttributeVal(Owner, EffectCfg.AttributeType, modifierOutput, this);
                    break;
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
            _pool.Release(this);
        }

        #endregion
      
    }
}