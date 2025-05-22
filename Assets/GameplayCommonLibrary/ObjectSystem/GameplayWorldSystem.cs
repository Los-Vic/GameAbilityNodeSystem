using System;

namespace GameplayCommonLibrary
{
    //GameWorld里大部分Logic应该都在System里实现
    public class GameplayWorldSystem
    {
        public GameplayWorld World { get; private set; }
        public bool Enabled { get; set; }

        //用来实现System的继承替换逻辑
        public virtual Type GetRegisterType()
        {
            return GetType();
        }

        public virtual bool IsTickable()
        {
            return false;
        }
        
        public virtual void OnCreate(GameplayWorld world)
        {
            World = world;
            Enabled = true;
        }

        public virtual void Init()
        {
            
        }

        public virtual void UnInit()
        {
            
        }

        public virtual void Update(float dt)
        {
        }
    }
}