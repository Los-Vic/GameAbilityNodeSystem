using System;
using System.Collections.Generic;

namespace GAS.Logic.Target
{
    [Serializable]
    public class TargetQueryGameTagMultiple: TargetQueryMultipleBase
    {
        public List<EGameTag> WithAll;
        public List<EGameTag> WithAny;
        public List<EGameTag> WithNone;
    }

    public class TargetQueryGameTagUtility
    {
        public static bool GetQueryResult(GameAbility ability, TargetQueryGameTagMultiple cfg, ref List<GameUnit> units)
        {
            units.Clear();
            //With All
            var hasWithAll = cfg.WithAll is { Count: > 0 };
            if (hasWithAll)
            {
                foreach (var o in ability.System.GameTagSubsystem.GetGameTagInstance(cfg.WithAll[0]).Owners)
                {
                    if(o is GameUnit u)
                        units.Add(u);
                }

                for (var i = units.Count - 1; i >= 0; i--)
                {
                    var u = units[i];
                    var fail = false;
                    for (var j = 1 ; j < cfg.WithAll.Count; j++)
                    {
                        if(u.GetTagContainer().HasTag(cfg.WithAll[j]))
                            continue;
                        fail = true;
                    }

                    if (fail)
                        units.RemoveAt(i);
                }
            }
            
            //With Any
            var hasWithAny = cfg.WithAny is { Count: > 0 };
            if (hasWithAny)
            {
                if (hasWithAll)
                {
                    for (var i = units.Count - 1; i >= 0; i--)
                    {
                        var u = units[i];
                        var success = false;
                        foreach (var t in cfg.WithAny)
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
                    foreach (var t in cfg.WithAny)
                    {
                        foreach (var o in ability.System.GameTagSubsystem.GetGameTagInstance(t).Owners)
                        {
                            if(o is GameUnit u)
                                units.Add(u);
                        }
                    }
                }
            }
            
            //With None
            if (cfg.WithNone is { Count: > 0 })
            {
                if (!hasWithAny && !hasWithAll)
                    ability.System.GetAllGameUnits(ref units);
                
                for (var i = units.Count - 1; i >= 0; i--)
                {
                    var u = units[i];
                    var fail = false;
                    foreach (var t in cfg.WithNone)
                    {
                        if (!u.GetTagContainer().HasTag(t)) 
                            continue;
                        fail = true;
                        break;
                    }
                    if(fail)
                        units.RemoveAt(i);
                }
            }

            return units.Count > 0;
        }
    }
    
}