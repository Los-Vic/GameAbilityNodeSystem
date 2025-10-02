using System;
using UnityEngine;

namespace Gray.NG.Sample
{
    public class NodeGraphMgrAuthoring:MonoBehaviour
    {
        public RuntimeNodeGraph graph;
        private readonly NodeGraphManager _mgr = new();


        private void Start()
        {
            if(!graph)
                return;

            _mgr.Init();
            var director = _mgr.CreateDirector(graph);
            director.Start(typeof(RtEntryNode));
        }
    }
}