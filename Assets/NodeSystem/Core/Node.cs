using System;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public enum ENodeFunctionType
    {
        Value,
        Action,
        Entry,
    }
    
    [Serializable]
    public class Node
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        [SerializeField] private Rect position;

        [SerializeField]
        private string nodeName;
        [SerializeField]
        private ENodeFunctionType nodeFunctionType;

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
            return nodeFunctionType == ENodeFunctionType.Action;
        }
        
        public bool IsValueNode()
        {
            return nodeFunctionType == ENodeFunctionType.Value;
        }
        
        public bool IsEntryNode()
        {
            return nodeFunctionType == ENodeFunctionType.Entry;
        }
        
        public virtual string DisplayName() => nodeName;
        
        
#if UNITY_EDITOR

        public void InitNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null) 
                return;

            nodeName = nodeAttribute.Title;
            nodeFunctionType = nodeAttribute.FunctionType;
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