using System;

namespace GameplayCommonLibrary
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
    }
}