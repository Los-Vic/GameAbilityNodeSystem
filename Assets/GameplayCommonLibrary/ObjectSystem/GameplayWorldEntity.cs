using System.Collections.Generic;
using System;

namespace GameplayCommonLibrary
{
    //不允许继承，希望Entity就是一个比较单纯的数据容器。
    //拓展数据通过添加Component实现。
    public sealed class GameplayWorldEntity
    {
        private readonly Dictionary<Type, GameplayWorldComponent> _components = new();
        public uint EntityID { get; private set; }
        public void SetEntityID(uint entityID) => EntityID = entityID;
        
        #region Components Methods
        public bool AddComponent(GameplayWorldComponent component)
        {
            if (!_components.TryAdd(component.GetType(), component)) 
                return false;
            
            component.OnAdd(this);
            return true;
        }

        public T GetComponent<T>() where T:GameplayWorldComponent
        {
            return _components.TryGetValue(typeof(T), out var comp) ? (T)comp : null;
        }

        public GameplayWorldComponent GetComponent(Type componentType)
        {
            return _components.GetValueOrDefault(componentType);
        }

        public bool RemoveComponent<T>(out T component) where T:GameplayWorldComponent
        {
            component = GetComponent<T>();
            if (component == null)
                return false;
            RemoveComponent(component);
            return true;
        }

        public bool RemoveComponent(Type componentType, out GameplayWorldComponent component)
        {
            component = GetComponent(componentType);
            if (component == null)
                return false;
            RemoveComponent(component);
            return true;
        }
        
        private void RemoveComponent(GameplayWorldComponent comp)
        {
            _components.Remove(comp.GetType());
            comp.OnRemove();
        }

        public bool HasComponent<T>() where T : GameplayWorldComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool HasComponent(Type componentType)
        {
            return _components.ContainsKey(componentType);
        }

        public Dictionary<Type, GameplayWorldComponent>.ValueCollection GetComponents()
        {
            return _components.Values;
        }
        
        public void ClearComponents()
        {
            _components.Clear();
        }
        
        #endregion
       
    }
}