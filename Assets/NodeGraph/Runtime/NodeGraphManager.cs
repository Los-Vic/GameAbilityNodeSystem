using System;
using System.Collections.Generic;
using System.Reflection;
using Gameplay.Common;

namespace Gray.NG
{
    public class NodeGraphManager
    {
        public ClassObjectPoolCollection PoolCollection { get; private set; }
        
        private readonly List<GraphDirector> _tickableDirectors = new();
        private readonly List<GraphDirector> _tickingDirectors = new();
        
        public void Init()
        {
            PoolCollection = new();
        }

        public void UnInit()
        {
            _tickableDirectors.Clear();
            _tickingDirectors.Clear();
            PoolCollection.Clear();
            PoolCollection = null;
        }

        public void Tick(float dt)
        {
            _tickingDirectors.AddRange(_tickableDirectors);
            foreach (var director in _tickingDirectors)
            {
                director.Tick(dt);
            }
            _tickingDirectors.Clear();
        }
        
        // 可采用的优化：避免游戏进行中的毛刺、卡顿，较大的消耗的操作可以提前到Load阶段
        // 1. 反射查找 NodeExecutorType
        // 2. 对象池提前使用Activator.CreateInstance创建对象
        // private readonly Dictionary<Type, Type> _nodeToExecutorTypeLookup = new();
        //
        // public void Load(List<RuntimeNodeGraph> nodeGraphs)
        // {
        //     _nodeToExecutorTypeLookup.Clear();
        //     foreach (var nodeGraph in nodeGraphs)
        //     {
        //         foreach (var n in nodeGraph.nodes)
        //         {
        //             var t = n.GetType();
        //             if (_nodeToExecutorTypeLookup.ContainsKey(t))
        //                 continue;
        //             var attr = t.GetCustomAttribute<NodeAttribute>();
        //             _nodeToExecutorTypeLookup.Add(t, attr.ExecutorType);
        //             PoolCollection.PrepareObjects(attr.ExecutorType, ClassObjectPoolCollection.DefaultCapacity,
        //                 ClassObjectPoolCollection.DefaultMaxSize);
        //         }
        //     }
        // }
        //
        // public Type GetExecutorType(Type nodeType)
        // {
        //     return _nodeToExecutorTypeLookup.GetValueOrDefault(nodeType);
        // }
        
        public GraphDirector CreateDirector(RuntimeNodeGraph graph)
        {
            var director = PoolCollection.Get<GraphDirector>();
            director.Init(this, graph);
            if (graph.isTickable)
            {
                _tickableDirectors.Add(director);
            }
            return director;
        }

        public void DestroyDirector(GraphDirector director)
        {
            if (director.RuntimeNodeGraph.isTickable)
            {
                _tickableDirectors.Remove(director);
            }
            PoolCollection.Release(director);
        }
    }
}