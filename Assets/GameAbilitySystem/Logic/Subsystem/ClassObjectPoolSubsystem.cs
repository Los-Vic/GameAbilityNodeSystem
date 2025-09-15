using GCL;

namespace GAS.Logic
{
    public class ClassObjectPoolSubsystem:GameAbilitySubsystem
    {
        private ClassObjectPoolCollection _objectPoolCollection;

        public override void Init()
        {
            _objectPoolCollection = new ClassObjectPoolCollection();
        }
        
        public override void UnInit()
        {
            //ClassObjectPoolMgr.Clear();
        }

        internal T Get<T>() where T : GameAbilitySystemObject, new()
        {
            var instance =  _objectPoolCollection.Get<T>();
            instance.System = System;
            return instance;
        }

        internal void Release<T>(T obj) where T : GameAbilitySystemObject
        {
            _objectPoolCollection.Release(obj);
        }

        internal void Log() => _objectPoolCollection.Log();
    }
}