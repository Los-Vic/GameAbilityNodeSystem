namespace GameplayCommonLibrary
{
    //GameWorld里大部分Logic应该都在System里实现
    public class GameplayWorldSystem
    {
        public GameplayWorld World { get; private set; }
        
        public virtual void OnCreate(GameplayWorld world)
        {
            World = world;
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