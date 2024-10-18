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

        public ENodeCategory NodeCategory { get; private set; }

        public NodeAttribute(string title, string menuItem = "", ENodeCategory nodeCategory = ENodeCategory.Flow)
        {
            Title = title;
            MenuItem = menuItem;
            NodeCategory = nodeCategory;
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
        public Type PortType;
        public string PortName;
        public Orientation Orientation;
        public Port.Capacity PortCapacity;

        public PortAttribute(Direction portDirection, Type portType, string portName = "", Orientation orientation = Orientation.Horizontal, 
            Port.Capacity portCapacity = Port.Capacity.Single)
        {
            PortName = portName;
            PortType = portType;
            Orientation = orientation;
            PortDirection = portDirection;
            
            //暂不支持多端口
            PortCapacity = Port.Capacity.Single;
        }
    }
    
    
}