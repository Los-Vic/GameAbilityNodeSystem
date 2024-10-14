using System;
using System.Collections.Generic;
using GameAbilitySystem.Logic.ObjectPool;
using GameAbilitySystem.Logic.Observe;

namespace GameAbilitySystem.Logic.Attribute
{
    public struct CompositeAttributeCreateParam<T> where T:IEquatable<T>, IComparable<T>
    {
        public GameAbilitySystemCfg.ECompositeAttributeType Type;
        public List<SimpleAttribute<T>> SimpleAttributes;
        public Func<List<T>, T> ValEquation;
    }
    
    public class CompositeAttribute<T>:IPoolObject where T:IEquatable<T>, IComparable<T>
    {
        public GameAbilitySystemCfg.ECompositeAttributeType Type { get; private set; }

        private List<SimpleAttribute<T>> _simpleAttributes;
        private Func<List<T>, T> _valEquation;
        private readonly List<T> _simpleAttributeVals = new();
        public readonly Observable<AttributeChangeMsg<T>> OnValChanged = new();

        public T Val
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
        
        public void Init(ref CompositeAttributeCreateParam<T> param)
        {
            if(_simpleAttributes == null)
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

                    OnValChanged.NotifyObservers(new AttributeChangeMsg<T>()
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
            OnValChanged.Reset();
        }
        
        #region Pool Interface

        public ObjectPoolParam GetPoolParam()
        {
            return new ObjectPoolParam()
            {
                Capacity = GameAbilitySystemCfg.PoolSizeDefine.DefaultCapacity,
                MaxSize = GameAbilitySystemCfg.PoolSizeDefine.DefaultMaxSize
            };
        }

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