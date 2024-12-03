using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NS
{
    public class NodeGraphAsset:ScriptableObject
    {
        [SerializeReference]
        //[HideInInspector]
        public List<Node> nodes = new();
        [SerializeReference] 
        public List<NodePort> ports = new();
        
#if UNITY_EDITOR
        private Dictionary<string, Node> _nodeMap = new();
        private Dictionary<string, NodePort> _portMap = new();

        public Node GetNode(string id) => _nodeMap.GetValueOrDefault(id);
        public NodePort GetPort(string id) => _portMap.GetValueOrDefault(id);
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

        public void AddNode(Node node, bool needCreatePorts = true)
        {
            nodes.Add(node);
            _nodeMap.Add(node.Id, node);
            
            //Create Ports
            if(!needCreatePorts)
                return;
            
            var type = node.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var attribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                if(attribute == null)
                    continue;
                var port = new NodePort(node.Id, attribute.PortDirection, attribute.PortType);
                fieldInfo.SetValue(node, port.Id);
                AddPort(port);
            }
        }

        public void RemoveNode(Node node)
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
                    RemovePort(p);
                    break;
                }
            }
        }

        public void AddPort(NodePort port)
        {
            ports.Add(port);
            _portMap.Add(port.Id, port);
        }

        public void RemovePort(NodePort port)
        {
            ports.Remove(port);
            _portMap.Remove(port.Id);
        }

        public bool HasNodeName(string nodeName)
        {
            foreach (var n in nodes)
            {
                if (n.nodeName != nodeName) 
                    continue;
                return true;
            }

            return false;
        }
#endif
    }
}