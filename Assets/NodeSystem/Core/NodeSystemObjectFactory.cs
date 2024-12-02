﻿using System;
using System.Collections.Generic;
using System.Reflection;
using CommonObjectPool;

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
            _objectPoolMgr.Clear();
        }
        
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

        public virtual void DestroyNodeRunner(NodeSystemNodeRunner runner)
        {
            if(runner == NodeSystemNodeRunner.DefaultRunner)
                return;
            _objectPoolMgr.DestroyObject(runner);
        }

        public virtual NodeSystemGraphRunner CreateGraphRunner()
        {
            return _objectPoolMgr.CreateObject<NodeSystemGraphRunner>();
        }

        public virtual void DestroyGraphRunner(NodeSystemGraphRunner runner)
        {
            _objectPoolMgr.DestroyObject(runner);
        }
    }
}