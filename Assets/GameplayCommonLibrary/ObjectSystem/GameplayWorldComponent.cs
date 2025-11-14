namespace Gameplay.Common
{
    public class GameplayWorldComponent
    {
        public virtual void OnAdd(GameplayWorldEntity entity)
        {
        }
        public virtual void OnRemove()
        {
        }
    }

    public class GameplayWorldComponentWithEntity : GameplayWorldComponent
    {
        public GameplayWorldEntity Entity { get; private set; }
        
        public override void OnAdd(GameplayWorldEntity entity)
        {
            Entity = entity;
        }

        public override void OnRemove()
        {
            Entity = null;
        }
    }
}