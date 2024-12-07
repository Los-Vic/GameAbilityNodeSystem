﻿using System;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public enum ENodeCategory
    {
        //------Flow Node Start------
        Event = 0,
        FlowControl = 1,
        //Executable
        ExecInstant = 100,
        ExecDebugInstant = 101,
        ExecNonInstant = 102,
        //------Flow Node End------
        
        //------Value Node Start------
        Value = 200,
        //------Value Node End------
    }

    public enum ENodeNumsLimit
    {
        None,
        Singleton, 
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
        
        public bool IsExecNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.NodeCategory is (int)ENodeCategory.ExecInstant or (int)ENodeCategory.ExecDebugInstant
                or (int)ENodeCategory.ExecNonInstant or (int)ENodeCategory.FlowControl;
        }

        public bool IsValueNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.NodeCategory is (int)ENodeCategory.Value;
        }

        public bool IsEventNode()
        {
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute == null)
            {
                return false;
            }

            return nodeAttribute.NodeCategory is (int)ENodeCategory.Event;
        }

        public virtual string DisplayName() => nodeName;
    }
}