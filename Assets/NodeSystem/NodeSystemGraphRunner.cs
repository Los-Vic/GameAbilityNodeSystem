using System.Reflection;
using UnityEngine;

namespace NodeSystem
{
    public class NodeSystemGraphRunner:MonoBehaviour
    {
        public NodeSystemGraphAsset asset;

        private void Start()
        {
            NodeSystemNode startNode = null;
            foreach (var node in asset.nodes)
            {
                var attribute = node.GetType().GetCustomAttribute<NodeAttribute>();
                if (attribute is not { NodeCategory: ENodeCategory.Start }) 
                    continue;
                startNode = node;
                break;
            }
            if(startNode == null)
                return;
            
            
        }
    }
}