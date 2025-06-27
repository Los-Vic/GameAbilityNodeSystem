using System.Collections.Generic;

namespace GAS.Logic.Target
{
    public static class TargetQueryUtility
    {
        public static bool GetTargetFromAbility(GameAbility ability, TargetQuerySingleBase cfg, out GameUnit target,
            bool ignoreSelf = false)
        {
            return ability.Owner.Sys.TargetSearcher.GetTargetFromAbility(ability, cfg, out target, ignoreSelf);
        }

        public static bool GetTargetsFromAbility(GameAbility ability, TargetQueryMultipleBase cfg,
            ref List<GameUnit> targets, bool ignoreSelf = false)
        {
            switch (cfg)
            {
                case TargetQueryGameTagMultiple t:
                    return TargetQueryGameTagUtility.GetQueryResult(ability, t, ref targets, ignoreSelf);
            }
            return ability.Owner.Sys.TargetSearcher.GetTargetsFromAbility(ability, cfg, ref targets, ignoreSelf);
        }
    }
}