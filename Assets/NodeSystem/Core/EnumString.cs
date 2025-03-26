using System.Collections.Generic;
using UnityEngine;

namespace NS
{
    public class EnumStringAttribute:PropertyAttribute
    {
        public readonly string ProviderAsset;

        public EnumStringAttribute(string providerAsset)
        {
            ProviderAsset = providerAsset;
        }
    }
    
    public interface IEnumStringProvider
    {
        public List<string> GetEnumStringList();
    }

}