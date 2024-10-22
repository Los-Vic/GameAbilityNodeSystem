using System;
using System.Collections.Generic;
using System.Reflection;

namespace NodeSystem
{
    public class NodeSystemNodeRunnerFactory
    {
        private readonly Dictionary<Type, Type> _cachedNodeToNodeRunnerTypeMap = new();
        
        public virtual NodeSystemNodeRunner CreateNodeRunner(Type type)
        {
            if (!_cachedNodeToNodeRunnerTypeMap.TryGetValue(type, out var runnerType))
            {
                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute == null)
                {
                    return NodeSystemNodeRunner.DefaultRunner;
                }

                runnerType = nodeAttribute.NodeRunnerType;
                _cachedNodeToNodeRunnerTypeMap.Add(type, runnerType);
            }

            var runner = Activator.CreateInstance(runnerType) as NodeSystemNodeRunner;
            if(runner == null)
                return NodeSystemNodeRunner.DefaultRunner;

            return runner;
        }
    }
}