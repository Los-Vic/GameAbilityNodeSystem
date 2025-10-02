using System.Collections.Generic;
using UnityEngine;

namespace Gray.NG
{
    public class RuntimeNodeGraph:ScriptableObject
    {
        public bool isTickable;
        [SerializeReference]
        public List<RuntimeNode> nodes = new();
        public List<int> entryNodeIndexed = new();
    }
}