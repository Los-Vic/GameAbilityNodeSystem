using System;

namespace NodeSystem
{
    public class NodeSystemNodeRunnerFactory
    {
        public virtual NodeSystemNodeRunner CreateNodeRunner(Type type)
        {
            return default;
        }
    }
}