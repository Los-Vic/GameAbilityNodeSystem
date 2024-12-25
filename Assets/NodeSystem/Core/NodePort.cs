using System;
using UnityEngine;

namespace NS
{
    public enum EPortDirection
    {
        Input,
        Output,
    }
    
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
        public EPortDirection direction;
        public string portType;
        public bool isFlowPort;
        
        public NodePort(string belongNodeId, EPortDirection direction, Type portType, bool isFlowPort, string connectPortId = null)
        {
            this.belongNodeId = belongNodeId;
            this.direction = direction;
            this.connectPortId = connectPortId;
            this.portType = portType.AssemblyQualifiedName;
            this.isFlowPort = isFlowPort;
        }

        public static bool IsValidPortId(string portId) => !string.IsNullOrEmpty(portId);
        public bool IsConnected() => !string.IsNullOrEmpty(connectPortId);
        public void ConnectTo(string portId) => connectPortId = portId;
        public void Disconnect() => connectPortId = null;
        public bool IsFlowPort() => isFlowPort;
    }
}