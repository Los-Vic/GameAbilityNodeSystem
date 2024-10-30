using System;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public enum ENodeCategory
    {
        //Flow
        Start = 0,
        Event = 1,
        FlowControl = 2,
        //Executable
        ExecInstant = 100,
        ExecDebugInstant = 101,
        ExecNonInstant = 102,
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

            return nodeAttribute.NodeCategory is ENodeCategory.ExecInstant or ENodeCategory.ExecDebugInstant
                or ENodeCategory.ExecNonInstant;
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