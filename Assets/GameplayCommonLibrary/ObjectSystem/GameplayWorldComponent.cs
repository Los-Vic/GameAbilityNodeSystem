namespace GameplayCommonLibrary
{
    public class GameplayWorldComponent
    {
        public GameplayWorldEntity Entity { get; private set; }
        
        public virtual void OnAdd(GameplayWorldEntity entity)
        {
            Entity = entity;
        }

        public virtual void OnRemove()
        {
            Entity = null;
        }
    }
}