using System;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public enum ENodeType
    {
        Value = 0,
        Action = 1,
        Entry = 2,
    }
    
    [Serializable]
    public class Node
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        [SerializeField] private Rect position;

        [SerializeField]
        private string nodeName;
        [SerializeField]
        private ENodeType nodeType;

        public string NodeName => nodeName;

        public string Id => guid;
        public Rect Position
        {
            get => position;
            set => position = value;
        }
        
        public static bool IsValidNodeId(string nodeId) => !string.IsNullOrEmpty(nodeId);
        
        public bool IsActionNode()
        {
            return nodeType == ENodeType.Action;
        }
        
        public bool IsValueNode()
        {
            return nodeType == ENodeType.Value;
        }
        
        public bool IsEntryNode()
        {
            return nodeType == ENodeType.Entry;
        }

        public override string ToString()
        {
            return nodeName;
        }
#if UNITY_EDITOR

        public void InitNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null) 
                return;

            nodeName = nodeAttribute.Title;
            nodeType = nodeAttribute.Type;
        }
        
        public void SetNodeName(string postfix)
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null) 
                return;
            
            nodeName = $"{nodeAttribute.Title}_{postfix}";
        }
#endif
    }
}