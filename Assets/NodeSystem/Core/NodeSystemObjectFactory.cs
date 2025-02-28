using System;
using System.Collections.Generic;
using System.Reflection;
using GameplayCommonLibrary;

namespace NS
{
    public class NodeSystemObjectFactory
    {
        private readonly Dictionary<Type, Type> _cachedNodeToNodeRunnerTypeMap = new();
        private readonly ClassObjectPoolMgr _classObjectPoolMgr;

        public NodeSystemObjectFactory(ClassObjectPoolMgr classObjectPoolMgr)
        {
            _classObjectPoolMgr = classObjectPoolMgr;
        }
        
        public void Clear()
        {
            _cachedNodeToNodeRunnerTypeMap.Clear();
            _classObjectPoolMgr.Clear();
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

            var runner = _classObjectPoolMgr.Get(runnerType) as NodeRunner;
            return runner ?? NodeRunner.DefaultRunner;
        }

        public virtual void DestroyNodeRunner(NodeRunner runner)
        {
            if(runner == NodeRunner.DefaultRunner)
                return;
            _classObjectPoolMgr.Release(runner);
        }

        public NodeGraphRunner CreateGraphRunner()
        {
            return _classObjectPoolMgr.Get<NodeGraphRunner>() ;
        }

        public virtual void DestroyGraphRunner(NodeGraphRunner runner)
        {
            _classObjectPoolMgr.Release(runner);
        }
    }
}