namespace GAS.Logic
{
    public class GameAbilitySubsystem
    {
        internal GameAbilitySystem System { get; private set; }

        public virtual void OnCreate(GameAbilitySystem system)
        {
            System = system;
        }

        public virtual void Init()
        {
        }

        public virtual void UnInit()
        {
        }

        public virtual void Update(float deltaTime)
        {
            
        }
    }
}