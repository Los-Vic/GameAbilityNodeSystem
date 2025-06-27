using System.Collections.Generic;
using UnityEngine;

namespace GAS
{
    [CreateAssetMenu(fileName = "DebugCreateUnitsTemplate", menuName = "GameAbilitySystem/Debug/DebugCreateUnitsTemplate")]
    public class DebugCreateUnitsTemplate:ScriptableObject
    {
        public List<CreateUnitContext> unitContexts;
    }
}