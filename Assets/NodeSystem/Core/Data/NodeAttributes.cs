using System;

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
        public ENodeType Type { get; private set; }
        
        public string ToolTip { get; private set; }

        public NodeAttribute(string title, string menuItem, ENodeType type, Type runnerType, 
            int nodeCategory = 0, int scope = 0, string tooltip = "")
        {
            Title = title;
            MenuItem = menuItem;
            NodeCategory = nodeCategory;
            NodeRunnerType = runnerType;
            Scope = scope;
            Type = type;
            ToolTip = tooltip;
            
            //暂时不考虑这个
            IsSingleton = false;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExposedAttribute:Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EntryAttribute:Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PortAttribute : Attribute
    {
        public readonly EPortDirection PortDirection;
        public readonly Type PortType;
        public readonly string PortName;
        public readonly bool IsFlowPort;

        public PortAttribute(EPortDirection portDirection, Type portType, string portName = "")
        {
            PortName = portName;
            PortType = portType;
            PortDirection = portDirection;
            IsFlowPort = typeof(BaseFlowPort).IsAssignableFrom(portType);
        }
    }
}