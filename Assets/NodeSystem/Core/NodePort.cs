using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NS
{
    /// <summary>
    /// 暂不支持多线端口
    /// </summary>
    [Serializable]
    public class NodePort
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        public string Id => guid;
        public string belongNodeId;
        public string connectPortId;
        public Direction direction;
        public string portType;
        
        public NodePort(string belongNodeId, Direction direction, Type portType, string connectPortId = null)
        {
            this.belongNodeId = belongNodeId;
            this.direction = direction;
            this.connectPortId = connectPortId;
            this.portType = portType.AssemblyQualifiedName;
        }

        public static bool IsValidPortId(string portId) => !string.IsNullOrEmpty(portId);
        public bool IsConnected() => !string.IsNullOrEmpty(connectPortId);
        public void ConnectTo(string portId) => connectPortId = portId;
        public void Disconnect() => connectPortId = null;
        public bool IsFlowPort() => portType == typeof(ExecutePort).AssemblyQualifiedName;
    }
}