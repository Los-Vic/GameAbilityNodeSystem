using System;
using System.Reflection;
using UnityEngine;

namespace NodeSystem
{
    public enum ENodeCategory
    {
        //Flow Start
        Start = 0,
        Event = 1,
        //Flow 
        FlowInstant = 100,
        DebugFlowInstant = 101,
        FlowNonInstant = 102,
        //Value
        Value = 200,
    }

    public enum ENodeNumsLimit
    {
        None,
        Singleton, 
    }
    
    [Serializable]
    public class NodeSystemNode
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

        public NodeSystemNode()
        {
#if UNITY_EDITOR
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute != null)
            {
                nodeName = nodeAttribute.Title;
            }
#endif
        }

        public bool IsFlowNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.NodeCategory is ENodeCategory.FlowInstant or ENodeCategory.DebugFlowInstant
                or ENodeCategory.FlowNonInstant;
        }

        public bool IsValueNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.NodeCategory is ENodeCategory.Value;
        }
    }
}