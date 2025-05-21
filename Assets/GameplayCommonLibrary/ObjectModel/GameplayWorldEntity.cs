using System.Collections.Generic;
using System;

namespace GameplayCommonLibrary
{
    //希望Entity就是一个比较单纯的数据容器。
    //看看能不能让Entity不要引用World。如果需要World，则从System里通过方法注入
    public class GameplayWorldEntity
    {
        private readonly Dictionary<Type, GameplayWorldComponent> _components = new();
        
        #region Components
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
        
        #endregion
    }
}