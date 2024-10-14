using System;

namespace NodeSystem
{
    [Serializable]
    public class NodeSystemConnection
    {
        public NodeSystemConnectionPort inPort;
        public NodeSystemConnectionPort outPort;

        public NodeSystemConnection(string inNodeId, int inPortIndex, string outNodeId, int outPortIndex)
        {
            inPort = new NodeSystemConnectionPort(inNodeId, inPortIndex);
            outPort = new NodeSystemConnectionPort(outNodeId, outPortIndex);
        }
    }

    [Serializable]
    public struct NodeSystemConnectionPort
    {
        public string nodeId;
        public int portIndex;

        public NodeSystemConnectionPort(string nodeId, int portIndex)
        {
            this.nodeId = nodeId;
            this.portIndex = portIndex;
        }
    }
}