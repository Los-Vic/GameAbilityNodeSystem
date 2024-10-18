using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NodeSystem
{
    [CreateAssetMenu(menuName = "NodeSystem/GraphAsset", fileName = "NewGraph")]
    public class NodeSystemGraphAsset:ScriptableObject
    {
        [Header("!!! Not Edit !!!")]
        [SerializeReference]
        //[HideInInspector]
        public List<NodeSystemNode> nodes = new();
        [SerializeReference] 
        public List<NodeSystemPort> ports = new();
        
#if UNITY_EDITOR
        private Dictionary<string, NodeSystemNode> _nodeMap = new();
        private Dictionary<string, NodeSystemPort> _portMap = new();

        public NodeSystemNode GetNode(string id) => _nodeMap.GetValueOrDefault(id);
        public NodeSystemPort GetPort(string id) => _portMap.GetValueOrDefault(id);
        public void LoadMap()
        {
            _nodeMap.Clear();
            _portMap.Clear();
            foreach (var n in nodes)
            {
                _nodeMap.Add(n.Id, n);
            }

            foreach (var p in ports)
            {
                _portMap.Add(p.Id, p);
            }
        }

        public void AddNode(NodeSystemNode node)
        {
            nodes.Add(node);
            _nodeMap.Add(node.Id, node);
            
            //Create Ports
            var type = node.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var attribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                if(attribute == null)
                    continue;
                var port = new NodeSystemPort(node.Id, attribute.PortDirection);
                fieldInfo.SetValue(node, port.Id);
                ports.Add(port);
                _portMap.Add(port.Id, port);
            }
        }

        public void RemoveNode(NodeSystemNode node)
        {
            nodes.Remove(node);
            _nodeMap.Remove(node.Id);
            
            //Remove Ports
            var type = node.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var attribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                if(attribute == null)
                    continue;
                var portId = (string)fieldInfo.GetValue(node);
                foreach (var p in ports.Where(p => portId == p.Id))
                {
                    ports.Remove(p);
                    _portMap.Remove(p.Id);
                    break;
                }
            }
        }
#endif
    }
}