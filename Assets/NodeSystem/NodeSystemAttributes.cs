using System;

namespace NodeSystem
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute : Attribute
    {
        public string Title { get; private set; }
        public string MenuItem { get; private set; }
        public uint InPortNums { get; private set; }
        public uint OutPortNums { get; private set; }

        public NodeAttribute(string title, string menuItem = "", uint inPortNums = 1, uint outPortNums = 1)
        {
            Title = title;
            MenuItem = menuItem;
            InPortNums = inPortNums;
            OutPortNums = outPortNums;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExposedPropAttribute:Attribute
    {
    }
    
}