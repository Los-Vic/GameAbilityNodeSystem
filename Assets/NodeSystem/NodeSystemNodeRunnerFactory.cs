using System;
using System.Collections.Generic;
using System.Reflection;
using NodeSystem.ObjectPool;

namespace NodeSystem
{
    public class NodeSystemNodeRunnerFactory
    {
        private readonly Dictionary<Type, Type> _cachedNodeToNodeRunnerTypeMap = new();
        private readonly ObjectPoolMgr _objectPoolMgr = new();
        
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

            var runner = _objectPoolMgr.CreateObject(runnerType) as NodeSystemNodeRunner;
            return runner ?? NodeSystemNodeRunner.DefaultRunner;
        }
    }
}