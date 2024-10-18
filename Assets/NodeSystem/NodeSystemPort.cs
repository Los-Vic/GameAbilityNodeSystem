using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NodeSystem
{
    /// <summary>
    /// 暂不支持多线端口
    /// </summary>
    [Serializable]
    public class NodeSystemPort
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        public string Id => guid;
        
        public string belongNodeId;
        public string connectPortId;
        public Direction direction;

        public NodeSystemPort(string belongNodeId, Direction direction)
        {
            this.belongNodeId = belongNodeId;
            this.direction = direction;
        }

        public void ConnectTo(string portId) => connectPortId = portId;
        public void Disconnect() => connectPortId = null;
    }
}