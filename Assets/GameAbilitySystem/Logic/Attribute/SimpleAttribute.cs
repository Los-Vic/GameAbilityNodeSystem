using System;
using System.Collections.Generic;
using CommonObjectPool;
using NS;

namespace GameAbilitySystem.Logic
{
    public struct AttributeChangeMsg<T> where T:IEquatable<T>, IComparable<T>
    {
        public T OldVal;
        public T NewVal;
        public GameEffect<T> ChangedByEffect;
    }

    public struct SimpleAttributeCreateParam<T> where T:IEquatable<T>, IComparable<T>
    {
        public GameAbilitySystemCfg.ESimpleAttributeType Type;
        public T DefaultVal;
        public List<IValueDecorator<T>> Decorators;
    }
    
    public class SimpleAttribute<T>:IPoolObject where T:IEquatable<T>, IComparable<T>
    {
        public GameAbilitySystemCfg.ESimpleAttributeType Type { get; private set; }
        
        private T _val;
        private List<IValueDecorator<T>> _valDecorators;
        public readonly Observable<AttributeChangeMsg<T>> OnValChanged = new();
        
        public void Init(ref SimpleAttributeCreateParam<T> param)
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

        public T Val => _val;
        
        internal void SetVal(T newVal)
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