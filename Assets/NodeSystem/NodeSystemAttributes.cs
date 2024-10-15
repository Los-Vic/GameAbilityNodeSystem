using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string Title { get; private set; }
        public string MenuItem { get; private set; }

        public NodeAttribute(string title, string menuItem = "")
        {
            Title = title;
            MenuItem = menuItem;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExposedPropAttribute:Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PortAttribute : Attribute
    {
        public Direction PortDirection;
        public Orientation Orientation;
        public Port.Capacity PortCapacity;

        public PortAttribute(Direction portDirection, Orientation orientation = Orientation.Horizontal, 
            Port.Capacity portCapacity = Port.Capacity.Single)
        {
            Orientation = orientation;
            PortDirection = portDirection;
            PortCapacity = portCapacity;
        }
    }
}