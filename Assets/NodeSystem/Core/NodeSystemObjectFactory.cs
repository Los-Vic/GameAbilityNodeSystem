using System;
using System.Collections.Generic;
using System.Reflection;
using GameplayCommonLibrary;

namespace NS
{
    public class NodeSystemObjectFactory
    {
        private readonly Dictionary<Type, Type> _cachedNodeToNodeRunnerTypeMap = new();
        private readonly ObjectPoolMgr _objectPoolMgr;

        public NodeSystemObjectFactory(ObjectPoolMgr objectPoolMgr)
        {
            _objectPoolMgr = objectPoolMgr;
        }
        
        public void Clear()
        {
            _cachedNodeToNodeRunnerTypeMap.Clear();
            _objectPoolMgr.Clear();
        }
        
        public NodeRunner CreateNodeRunner(Type type)
        {
            if (!_cachedNodeToNodeRunnerTypeMap.TryGetValue(type, out var runnerType))
            {
                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute == null)
                {
                    return NodeRunner.DefaultRunner;
                }

                runnerType = nodeAttribute.NodeRunnerType;
                if (runnerType == null)
                {
                    return NodeRunner.DefaultRunner;
                }
                _cachedNodeToNodeRunnerTypeMap.Add(type, runnerType);
            }

            var runner = _objectPoolMgr.Get(runnerType) as NodeRunner;
            return runner ?? NodeRunner.DefaultRunner;
        }

        public virtual void DestroyNodeRunner(NodeRunner runner)
        {
            if(runner == NodeRunner.DefaultRunner)
                return;
            _objectPoolMgr.Release(runner);
        }

        public NodeGraphRunner CreateGraphRunner()
        {
            return _objectPoolMgr.Get<NodeGraphRunner>() ;
        }

        public virtual void DestroyGraphRunner(NodeGraphRunner runner)
        {
            _objectPoolMgr.Release(runner);
        }
    }
}