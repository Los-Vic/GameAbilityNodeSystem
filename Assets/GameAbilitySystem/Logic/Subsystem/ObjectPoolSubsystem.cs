using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class ObjectPoolSubsystem:GameAbilitySubsystem
    {
        internal ObjectPoolMgr ObjectPoolMgr { get; private set; }

        public override void Init(GameAbilitySystem system)
        {
            base.Init(system);
            ObjectPoolMgr = new ObjectPoolMgr();
        }

        public override void Reset()
        {
            ObjectPoolMgr.Clear();
        }

        public override void UnInit()
        {
            ObjectPoolMgr.Clear();
        }
    }
}