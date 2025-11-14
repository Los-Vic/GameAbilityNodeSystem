using System;
using System.Collections.Generic;

namespace Gameplay.Common
{
    public interface IEntityMgr
    {
        public void OnCreate();
        public void Init();
        public void UnInit();
        public GameplayWorldEntity CreateEntity();
        public void DestroyEntity(GameplayWorldEntity entity);
        public GameplayWorldEntity GetEntity(uint entityID);
        public T CreateComponent<T>() where T : GameplayWorldComponent, new();
        public GameplayWorldComponent CreateComponent(Type componentType);
        public void DestroyComponent(GameplayWorldComponent component);
        public void GetAllComponents<T>(ref List<T> components) where T:GameplayWorldComponent;
        public void GetAllComponents(Type componentType, ref List<GameplayWorldComponent> components);
        public IReadOnlyList<GameplayWorldComponent> GetAllComponents(Type componentType);
        public string GetMgrDebugStats();
    }
}