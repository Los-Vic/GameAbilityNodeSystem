using System;

namespace GCL
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SystemAttribute : Attribute
    {
        public Type Parent; 
    }
}