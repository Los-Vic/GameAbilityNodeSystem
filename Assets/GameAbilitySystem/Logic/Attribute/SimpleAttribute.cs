using System.Collections.Generic;
using GameplayCommonLibrary;
using MissQ;

namespace GAS.Logic
{
    public struct AttributeChangeMsg
    {
        public FP OldVal;
        public FP NewVal;
        public GameEffect ChangedByEffect;
    }

    public struct AttributeChangeForCue
    {
        public FP OldVal;
        public FP NewVal;
    }

    public struct SimpleAttributeCreateParam
    {
        public ESimpleAttributeType Type;
        public FP DefaultVal;
        public List<ValueDecorator> Decorators;
    }
    
    [System.Serializable]
    public class SimpleAttribute:GameAbilitySystemObject
    {
        public ESimpleAttributeType Type { get; private set; }
        
        private FP _val;
        
        private List<ValueDecorator> _valDecorators;
        //for game logic
        internal readonly Observable<AttributeChangeMsg> OnValChanged = new();
        //for view, UI
        internal readonly Observable<AttributeChangeForCue> OnPlayValChangeCue = new();
        
        internal void Init(ref SimpleAttributeCreateParam param)
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
            OnValChanged.Clear();
            OnPlayValChangeCue.Clear();
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
        
        public override void OnReturnToPool()
        {
            UnInit();
        }
        
        #endregion
       
    }
}