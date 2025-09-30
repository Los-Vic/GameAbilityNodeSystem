using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.GraphToolkit.Editor;

namespace Gray.NG.Editor
{
    [Serializable]
    public abstract class GraphNode : Node
    {
        public readonly string guid = Guid.NewGuid().ToString();
        
        public virtual void AddInputOutputPorts(Type runtimeNodeType, IPortDefinitionContext context)
        {
            var fields = runtimeNodeType.GetFields();
            foreach (var fieldInfo in fields)
            {
                var inputPortAttr = fieldInfo.GetCustomAttribute<InputPortAttribute>();
                if (inputPortAttr != null)
                {
                    context.AddInputPort(fieldInfo.Name)
                        .WithDisplayName(inputPortAttr.DisplayName)
                        .WithConnectorUI(GetPortConnectorUI(inputPortAttr.ConnectorStyle))
                        .WithDataType(fieldInfo.FieldType)
                        .Build();
                }
                
                var outputPortAttr = fieldInfo.GetCustomAttribute<OutputPortAttribute>();
                if (outputPortAttr != null)
                {
                    context.AddOutputPort(fieldInfo.Name)
                        .WithDisplayName(outputPortAttr.DisplayName)
                        .WithConnectorUI(GetPortConnectorUI(outputPortAttr.ConnectorStyle))
                        .WithDataType(fieldInfo.FieldType)
                        .Build();
                }
            }
        }

        public virtual RuntimeNode CreateRuntimeNode()
        {
            return null;
        }
        
        public virtual void AssignRuntimeNodePortValues(RuntimeNode runtimeNode,
            Dictionary<INode, RuntimeNode> nodeMap)
        {
        }
        
        public static PortConnectorUI GetPortConnectorUI(EPortConnectorStyle style)
        {
            return style switch
            {
                EPortConnectorStyle.Arrowhead => PortConnectorUI.Arrowhead,
                EPortConnectorStyle.Circle => PortConnectorUI.Circle,
                _ => PortConnectorUI.Circle
            };
        }
        
        public static T GetInputPortValue<T>(IPort port)
        {
            T value = default;

            // If port is connected to another node, get value from connection
            if (port.isConnected)
            {
                switch (port.firstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                    default:
                        break;
                }
            }
            else
            {
                // If port has embedded value, return it.
                // Otherwise, return the default value of the port
                port.TryGetValue(out value);
            }

            return value;
        }
        
        public static INode GetNextNode(INode currentNode, string portName)
        {
            var outputPort = currentNode.GetOutputPortByName(portName);
            var nextNodePort = outputPort.firstConnectedPort;
            var nextNode = nextNodePort?.GetNode();
            return nextNode;
        }
    }
}