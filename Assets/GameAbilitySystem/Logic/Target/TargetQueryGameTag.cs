using System;
using System.Collections.Generic;

namespace GAS.Logic.Target
{
    [Serializable]
    public class TargetQueryGameTagMultiple: TargetQueryMultipleBase
    {
        public List<EGameTag> withAll;
        public List<EGameTag> withAny;
        public List<EGameTag> withNone;
    }

    public class TargetQueryGameTagUtility
    {
        public static bool GetQueryResult(GameAbility ability, TargetQueryGameTagMultiple cfg, ref List<GameUnit> units, bool ignoreSelf = false)
        {
            units.Clear();
            
            //With All
            var hasWithAll = cfg.withAll is { Count: > 0 };
            if (hasWithAll)
            {
                foreach (var o in ability.System.GameTagSubsystem.GetGameTagInstance(cfg.withAll[0]).Owners)
                {
                    if(o is GameUnit u && IgnoreSelfCheck(u))
                        units.Add(u);
                }

                for (var i = units.Count - 1; i >= 0; i--)
                {
                    var u = units[i];
                    var fail = false;
                    for (var j = 1 ; j < cfg.withAll.Count; j++)
                    {
                        if(u.GetTagContainer().HasTag(cfg.withAll[j]))
                            continue;
                        fail = true;
                    }

                    if (fail)
                        units.RemoveAt(i);
                }
            }
            
            //With Any
            var hasWithAny = cfg.withAny is { Count: > 0 };
            if (hasWithAny)
            {
                if (hasWithAll)
                {
                    for (var i = units.Count - 1; i >= 0; i--)
                    {
                        var u = units[i];
                        var success = false;
                        foreach (var t in cfg.withAny)
                        {
                            if (!u.GetTagContainer().HasTag(t))
                                continue;
                            
                            success = true;
                            break;
                        }
                        if (!success)
                            units.RemoveAt(i);
                    }
                }
                else
                {
                    foreach (var t in cfg.withAny)
                    {
                        foreach (var o in ability.System.GameTagSubsystem.GetGameTagInstance(t).Owners)
                        {
                            if(o is GameUnit u && IgnoreSelfCheck(u))
                                units.Add(u);
                        }
                    }
                }
            }
            
            //With None
            if (cfg.withNone is { Count: > 0 })
            {
                if (!hasWithAny && !hasWithAll)
                    ability.System.GetAllGameUnits(ref units);
                
                for (var i = units.Count - 1; i >= 0; i--)
                {
                    var u = units[i];
                    var fail = false;
                    foreach (var t in cfg.withNone)
                    {
                        if (!u.GetTagContainer().HasTag(t)) 
                            continue;
                        fail = true;
                        break;
                    }
                    if(fail || !IgnoreSelfCheck(u))
                        units.RemoveAt(i);
                }
            }

            return units.Count > 0;

            bool IgnoreSelfCheck(GameUnit u)
            {
                return !ignoreSelf || u != ability.Owner;
            }
        }
    }
    
}