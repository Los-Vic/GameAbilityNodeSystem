using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NS
{
    public enum ENodeFunctionType
    {
        Value,
        Flow,
        Portal,
    }
    
    [Serializable]
    public class Node
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        [SerializeField] private Rect position;

        public string nodeName;

        public string Id => guid;
        public Rect Position
        {
            get => position;
            set => position = value;
        }

        private readonly ENodeFunctionType _nodeFunctionType;
        
        public Node()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null) 
                return;
            
#if UNITY_EDITOR
            nodeName = nodeAttribute.Title;
#endif
            _nodeFunctionType = nodeAttribute.FunctionType;
        }
        
        public static bool IsValidNodeId(string nodeId) => !string.IsNullOrEmpty(nodeId);
        
        public bool IsFlowNode()
        {
            return _nodeFunctionType == ENodeFunctionType.Flow;
        }
        
        public bool IsValueNode()
        {
            return _nodeFunctionType == ENodeFunctionType.Value;
        }
        
        public bool IsPortalNode()
        {
            return _nodeFunctionType == ENodeFunctionType.Portal;
        }

        public virtual string DisplayName() => nodeName;
    }
}