using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class ObjectPoolSubsystem:GameAbilitySubsystem
    {
        internal ObjectPoolMgr ObjectPoolMgr { get; private set; }

        public override void Init()
        {
            ObjectPoolMgr = new ObjectPoolMgr();
        }
        
        public override void UnInit()
        {
            ObjectPoolMgr.Clear();
        }
    }
}