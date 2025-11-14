using System;

namespace Gameplay.Common
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SystemAttribute : Attribute
    {
        public Type Parent; 
    }
}