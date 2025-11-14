using System;

namespace Gameplay.Common
{
    //GameWorld里大部分Logic应该都在System里实现
    //OnCreate -> Init -> UnInit -> OnDestroy
    public class GameplayWorldSystem
    {
        public GameplayWorld World { get; private set; }
        public bool Enabled { get; set; }
        
        /// <summary>
        /// 排序值越大，Init和Update越靠后，UnInit和Destroy越靠前
        /// </summary>
        /// <returns></returns>
        public virtual int GetExecuteOrder()
        {
            return 0;
        }
        
        /// <summary>
        /// OnCreate的执行顺序由World的AddSystem决定
        /// </summary>
        /// <param name="world"></param>
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

        public virtual void OnDestroy()
        {
            
        }
    }

    public class GameplayWorldTickableSystem:GameplayWorldSystem
    {
        public virtual void Update(float dt)
        {
        }
    }
    
}