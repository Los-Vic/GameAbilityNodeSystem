using System;
using System.Reflection;
using UnityEngine;

namespace NodeSystem
{
    public enum ENodeCategory
    {
        Start,
        Flow,
        Value,
        Event
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
            var nodeAttribute = GetType().GetCustomAttribute<NodeAttribute>();
            if (nodeAttribute != null)
            {
                nodeName = nodeAttribute.Title;
            }
        }
    }
}