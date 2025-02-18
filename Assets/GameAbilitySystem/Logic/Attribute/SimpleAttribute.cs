using System;
using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;
using NS;

namespace GAS.Logic
{
    public struct AttributeChangeMsg
    {
        public FP OldVal;
        public FP NewVal;
        public GameEffect ChangedByEffect;
    }

    public struct SimpleAttributeCreateParam
    {
        public ESimpleAttributeType Type;
        public FP DefaultVal;
        public List<IValueDecorator> Decorators;
    }
    
    public class SimpleAttribute:IPoolObject
    {
        public ESimpleAttributeType Type { get; private set; }
        
        private FP _val;
        private List<IValueDecorator> _valDecorators;
        public readonly Observable<AttributeChangeMsg> OnValChanged = new();
        
        public void Init(ref SimpleAttributeCreateParam param)
        {
            Type = param.Type;
            _valDecorators = param.Decorators;
            _val = param.DefaultVal;

            if (_valDecorators == null) 
                return;
            
            foreach (var d in _valDecorators)
            {
                d.Process(_val, out _val);
            }
        }

        private void UnInit()
        {
            _valDecorators = null;
            OnValChanged.Reset();
        }

        public FP Val => _val;
        
        internal void SetVal(FP newVal)
        {
            _val = newVal;
            if (_valDecorators == null) return;
            foreach (var d in _valDecorators)
            {
                d.Process(_val, out _val);
            }
        }
        
        #region Pool Interface

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