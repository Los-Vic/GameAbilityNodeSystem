﻿using System.Collections.Generic;
using UnityEngine;

namespace NodeSystem
{
    [CreateAssetMenu(menuName = "NodeSystem", fileName = "NodeSystem/NewGraph")]
    public class NodeSystemGraphAsset:ScriptableObject
    {
        [SerializeReference]
        public List<NodeSystemNode> nodes = new();
        public List<NodeSystemConnection> connections = new();
        
#if UNITY_EDITOR        
        public List<NodeSystemNode> Nodes => nodes;
        public List<NodeSystemConnection> Connections => connections;

        public void AddNode(NodeSystemNode node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(NodeSystemNode node)
        {
            nodes.Remove(node);
        }

        public void AddConnection(NodeSystemConnection connection)
        {
            connections.Add(connection);
        }

        public void RemoveConnection(NodeSystemConnection connection)
        {
            connections.Remove(connection);
        }
#endif
    }
}