using GameplayCommonLibrary;

namespace GAS.Logic
{
    public class ClassObjectPoolSubsystem:GameAbilitySubsystem
    {
        internal ClassObjectPoolMgr ClassObjectPoolMgr { get; private set; }

        public override void Init()
        {
            ClassObjectPoolMgr = new ClassObjectPoolMgr();
        }
        
        public override void UnInit()
        {
            //ClassObjectPoolMgr.Clear();
        }
    }
}