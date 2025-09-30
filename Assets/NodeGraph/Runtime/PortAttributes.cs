using System;
namespace Gray.NG
{
    public enum EPortConnectorStyle
    {
        Circle,
        Arrowhead,
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InputPortAttribute:Attribute
    {
        public string DisplayName = string.Empty;
        public EPortConnectorStyle ConnectorStyle;
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class OutputPortAttribute:Attribute
    {
        public string DisplayName = string.Empty;
        public EPortConnectorStyle ConnectorStyle;
    }
}