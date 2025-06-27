using System.Collections.Generic;

namespace GAS.Logic.Target
{
    public interface ITargetSearcher
    {
        bool GetTargetFromAbility(GameAbility ability, TargetQuerySingleBase cfg, out GameUnit target, bool ignoreSelf = false);
        bool GetTargetsFromAbility(GameAbility ability, TargetQueryMultipleBase cfg, ref List<GameUnit> targets, bool ignoreSelf = false);
    }
}