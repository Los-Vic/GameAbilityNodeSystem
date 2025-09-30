using System;
namespace Gray.NG
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeAttribute:Attribute
    {
        public Type ExecutorType;
    }
}