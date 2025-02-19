namespace GAS.Logic
{
    public class GameAbilitySubsystem
    {
        public GameAbilitySystem System { get; private set; }

        public virtual void Init(GameAbilitySystem system)
        {
            System = system;
        }

        public virtual void UnInit()
        {
            
        }

        public virtual void Reset()
        {
            
        }

        public virtual void Update(float deltaTime)
        {
            
        }
    }
}