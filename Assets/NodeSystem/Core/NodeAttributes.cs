using System;
using UnityEditor.Experimental.GraphView;

namespace NS
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        /// <summary>
        /// 节点标题
        /// </summary>
        public string Title { get; private set; }  
        /// <summary>
        /// 节点菜单路径
        /// </summary>
        public string MenuItem { get; private set; }
        /// <summary>
        /// 节点分组，主要用来区分颜色
        /// </summary>
        public int NodeCategory { get; private set; }
        /// <summary>
        /// 一个图里是否只允许一个该节点
        /// </summary>
        public bool IsSingleton { get; private set; }
        /// <summary>
        /// 节点的功能类型
        /// </summary>
        public Type NodeRunnerType { get; private set; }
        /// <summary>
        /// 节点的范围，用来过滤节点搜索
        /// </summary>
        public int Scope { get; private set; }
        /// <summary>
        /// 节点的作用类型
        /// </summary>
        public ENodeFunctionType FunctionType { get; private set; }

        public NodeAttribute(string title, string menuItem, ENodeFunctionType functionType, Type runnerType, 
            int nodeCategory = 0, int scope = 0, bool isSingleton = false)
        {
            Title = title;
            MenuItem = menuItem;
            NodeCategory = nodeCategory;
            IsSingleton = isSingleton;
            NodeRunnerType = runnerType;
            Scope = scope;
            FunctionType = functionType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExposedPropAttribute:Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EventTypeAttribute:Attribute
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
        public bool IsFlowPort;

        public PortAttribute(Direction portDirection, Type portType, string portName = "", Orientation orientation = Orientation.Horizontal, 
            Port.Capacity portCapacity = Port.Capacity.Single)
        {
            PortName = portName;
            PortType = portType;
            Orientation = orientation;
            PortDirection = portDirection;
            IsFlowPort = typeof(BaseFlowPort).IsAssignableFrom(portType);
            //暂不支持多端口
            PortCapacity = Port.Capacity.Single;
        }
    }
    
    
}