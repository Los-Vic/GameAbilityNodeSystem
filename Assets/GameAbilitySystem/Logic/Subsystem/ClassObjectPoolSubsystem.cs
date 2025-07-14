using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class ClassObjectPoolSubsystem:GameAbilitySubsystem
    {
        private ClassObjectPoolMgr _objectPoolMgr;

        public override void Init()
        {
            _objectPoolMgr = new ClassObjectPoolMgr();
        }
        
        public override void UnInit()
        {
            //ClassObjectPoolMgr.Clear();
        }

        internal T Get<T>() where T : GameAbilitySystemObject, new()
        {
            var instance =  _objectPoolMgr.Get<T>();
            instance.System = System;
            return instance;
        }

        internal void Release<T>(T obj) where T : GameAbilitySystemObject
        {
            _objectPoolMgr.Release(obj);
        }

        internal void Log() => _objectPoolMgr.Log();
    }
}