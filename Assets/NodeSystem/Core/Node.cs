using System;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public enum ENodeFunctionType
    {
        Value,
        Flow,
        Event,
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

        public Node()
        {
#if UNITY_EDITOR
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute != null)
            {
                nodeName = nodeAttribute.Title;
            }
#endif
        }

        public static bool IsValidNodeId(string nodeId) => !string.IsNullOrEmpty(nodeId);
        
        public bool IsFlowNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.FunctionType == ENodeFunctionType.Flow;
        }

        public bool IsValueNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.FunctionType == ENodeFunctionType.Value;
        }

        public bool IsEventNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.FunctionType == ENodeFunctionType.Event;
        }

        public virtual string DisplayName() => nodeName;
    }
}