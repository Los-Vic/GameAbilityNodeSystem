using System.Collections.Generic;

namespace GAS.Logic.Target
{
    public static class TargetSelectUtility
    {
        public static bool GetTargetFromAbility(GameAbility ability, TargetSelectSingleBase cfg, GameUnit targets)
        {
            return false;
        }
        
        public static bool GetTargetsFromAbility(GameAbility ability, TargetSelectMultipleBase cfg, ref List<GameUnit> targets)
        {
            return false;
        }
        
        public static bool GetTargetFromEffect(GameEffect effect, TargetSelectSingleBase cfg, GameUnit targets)
        {
            return false;
        }
        
        public static bool GetTargetsFromEffect(GameEffect effect, TargetSelectMultipleBase cfg, ref List<GameUnit> targets)
        {
            return false;
        }
    }
}