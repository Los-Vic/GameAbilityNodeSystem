using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public struct CompositeAttributeCreateParam
    {
        public ECompositeAttributeType Type;
        public List<SimpleAttribute> SimpleAttributes;
        public Func<List<FP>, FP> ValEquation;
    }
    
    public class CompositeAttribute:GameAbilitySystemObject
    {
        public ECompositeAttributeType Type { get; private set; }

        private List<SimpleAttribute> _simpleAttributes;
        private Func<List<FP>, FP> _valEquation;
        private readonly List<FP> _simpleAttributeVals = new();
        internal readonly Observable<AttributeChangeMsg> OnValChanged = new();
        internal readonly Observable<AttributeChangeForCue> OnPlayValChangeCue = new();
        public FP Val
        {
            get
            {
                if (_simpleAttributes == null || _valEquation == null)
                    return default;
                
                _simpleAttributeVals.Clear();
                foreach (var a in _simpleAttributes)
                {
                    _simpleAttributeVals.Add(a.Val);
                }

                return _valEquation.Invoke(_simpleAttributeVals);
            }
        }
        
        public void Init(ref CompositeAttributeCreateParam param)
        {
            if(param.SimpleAttributes == null)
                return;
            
            Type = param.Type;
            _simpleAttributes = param.SimpleAttributes;
            _valEquation = param.ValEquation;
            
            //组合属性不可设置数值，只会跟随基础属性的值变化而变化
            for (var i = 0; i<_simpleAttributes.Count; ++i)
            {
                var attributeIndex = i;
                var attribute = _simpleAttributes[i];
                attribute.OnValChanged.RegisterObserver(this, (data) =>
                {
                    _simpleAttributeVals.Clear();
                    foreach (var a in _simpleAttributes)
                    {
                        _simpleAttributeVals.Add(a.Val);
                    }
                    
                    _simpleAttributeVals[attributeIndex] = data.OldVal;
                    var oldVal = _valEquation.Invoke(_simpleAttributeVals);
                    _simpleAttributeVals[attributeIndex] = data.NewVal;
                    var newVal = _valEquation.Invoke(_simpleAttributeVals);

                    OnValChanged.NotifyObservers(new AttributeChangeMsg()
                    {
                        OldVal = oldVal,
                        NewVal = newVal,
                        ChangedByEffect = data.ChangedByEffect
                    });
                } );
            }
        }

        private void UnInit()
        {
            if (_simpleAttributes != null)
            {
                foreach (var a in _simpleAttributes)
                {
                    a.OnValChanged.UnRegisterObserver(this);
                }
                _simpleAttributes = null;
            }
            
            _valEquation = null;
            _simpleAttributeVals.Clear();
            OnValChanged.Clear();
        }
        
        #region Pool Interface
        public override void OnReturnToPool()
        {
            UnInit();
        }

        #endregion
    }
}