using System.Collections.Generic;
using UnityEngine;

//todo:还没想好怎么较好的使用Handle
namespace GameplayCommonLibrary
{
    public interface IHandleManagedObj
    {
        void OnCreate();
        void OnDestroy();
    }
    
    //Handle包裹在struct里，包含类型，可类型检测
    public struct Handle<T> where T : IHandleManagedObj
    {
        public uint ID;

        public Handle(uint id)
        {
            ID = id;
        }

        public bool IsValid()
        {
            return ID != 0;
        }
    }

    //使用简单的Handle生成方法：自增. 需要保证CreateObject的次数不超过uint.MaxValue = 4294967295
    public class HandleManagedObjPool<T> where T : IHandleManagedObj, new()
    {
        private readonly Dictionary<uint, T> _lookup = new();
        private readonly List<T> _activeObjList = new();
        private readonly List<T> _deadObjList = new();
        
        private uint _objectIdCounter;
        
        public Handle<T> CreateObject()
        {
            _objectIdCounter++;
            
            if (_deadObjList.Count > 0)
            {
                var obj = _deadObjList[^1];
                obj.OnCreate();
                
                _lookup.Add(_objectIdCounter, obj);
                _deadObjList.RemoveAt(_deadObjList.Count - 1);
                _activeObjList.Add(obj);
            }
            else
            {
                var obj = new T();
                obj.OnCreate();
                _lookup.Add(_objectIdCounter, obj);
                _activeObjList.Add(obj);
            }
            
            return new Handle<T>(_objectIdCounter);
        }
        
        public void DestroyObject(Handle<T> handle)
        {
            if (!_lookup.TryGetValue(handle.ID, out T obj))
            {
                GameLogger.LogError($"Destroy object[{typeof(T)}] failed, could not find object id: {handle.ID}");
                return;
            }

            _activeObjList.Remove(obj);
            _deadObjList.Add(obj);
        }

        public T GetObject(Handle<T> handle)
        {
            return _lookup.GetValueOrDefault(handle.ID);
        }
    }
}