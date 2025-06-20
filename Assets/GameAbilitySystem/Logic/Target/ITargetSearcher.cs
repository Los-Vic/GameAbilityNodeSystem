using System.Collections.Generic;

namespace GAS.Logic.Target
{
    public interface ITargetSearcher
    {
        bool GetTargetFromAbility(GameAbility ability, TargetSelectSingleBase cfg, out GameUnit target);
        bool GetTargetsFromAbility(GameAbility ability, TargetSelectMultipleBase cfg, ref List<GameUnit> targets);
    }
}