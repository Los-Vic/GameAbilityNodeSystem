using System;

namespace GameplayCommonLibrary
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SystemAttribute : Attribute
    {
        public Type Parent; 
    }
}