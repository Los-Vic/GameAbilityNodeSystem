using System.Collections.Generic;
using UnityEngine;

namespace GameAbilitySystem.Logic
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/AttributePrototype/SecondClassAttributePrototype")]
    public class SecondClassAttributePrototype:AttributePrototype
    {
        [SerializeReference]
        public List<FirstClassAttributePrototype> firstClassAttributePrototypes = new List<FirstClassAttributePrototype>();
        
    }
}