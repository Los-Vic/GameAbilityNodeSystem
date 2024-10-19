using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeSystem.Editor.Elements
{
    public class NodeSystemEditorNode:Node
    {
        public NodeSystemNode Node { get; private set; }
        public readonly Dictionary<Port, string> ViewPortToNodePort = new();
        public readonly Dictionary<string, Port> NodePortToViewPort = new();

        private SerializedObject _graphAssetObject;
        private SerializedProperty _serializedNode;
        
        public NodeSystemEditorNode(NodeSystemNode node, SerializedObject graphAssetObject)
        {
            _graphAssetObject = graphAssetObject;
            AddToClassList("node-graph-node");
            Node = node;

            var type = node.GetType();
            var att = type.GetCustomAttribute<NodeAttribute>();
            ConstructTitle(type, att);
            

            //Add Class To Uss
            var depths = att.MenuItem.Split('/');
            foreach (var depth in depths)
            {
                AddToClassList(depth.ToLower().Replace(' ', '-'));
            }
            this.name = type.Name;
            
            
            foreach (var fieldInfo in type.GetFields())
            {
                //Create Ports
                var portAttribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                if (portAttribute != null)
                {
                    CreatePort(fieldInfo, portAttribute);
                }
                
                //Create Extensions
                var exposedPropAttribute = fieldInfo.GetCustomAttribute<ExposedPropAttribute>();
                if (exposedPropAttribute != null)
                {
                    var propertyField = DrawField(fieldInfo.Name);
                    propertyField?.RegisterValueChangeCallback(OnFieldChangeCallback);
                }
            }
            
            RefreshExpandedState();
        }

        private void ConstructTitle(Type type, NodeAttribute attribute)
        {
            title = attribute.Title;
            var box = new Box
            {
                style =
                {
                    backgroundColor = ElementColor.GetNodeColor(attribute.NodeCategory)
                }
            };
            titleContainer.Add(box);
            box.SendToBack();
            box.StretchToParentSize();
            
            var txtElement = titleContainer.Q<Label>();
            txtElement.style.fontSize = 16;
            txtElement.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>() { value = FontStyle.Bold };
        }
        
        private void CreatePort(FieldInfo fieldInfo, PortAttribute portAttribute)
        {
            var port = InstantiatePort(portAttribute.Orientation, portAttribute.PortDirection, portAttribute.PortCapacity,
                fieldInfo.FieldType);

            port.portName = portAttribute.PortName;
            port.portColor = ElementColor.GetPortColor(portAttribute.PortType);
            var nodePort = (string)fieldInfo.GetValue(Node);
            ViewPortToNodePort.Add(port, nodePort);
            NodePortToViewPort.Add(nodePort, port);

            if (portAttribute.PortDirection == Direction.Input)
            {
                inputContainer.Add(port);
            }
            else
            {
                outputContainer.Add(port);
            }
        }
        
        private void OnFieldChangeCallback(SerializedPropertyChangeEvent evt)
        {
            
        }

        public void SavePosition()
        {
            Node.Position = GetPosition();
        }

        private void FetchSerializedProperty()
        {
            var nodes = _graphAssetObject.FindProperty("nodes");
            if (!nodes.isArray) 
                return;
            for (var i = 0; i < nodes.arraySize; ++i)
            {
                var element = nodes.GetArrayElementAtIndex(i);
                var id = element.FindPropertyRelative("guid");
                if (id.stringValue == Node.Id)
                {
                    _serializedNode = element;
                }
            }
        }
        
        private PropertyField DrawField(string fieldInfoName)
        {
            if (_serializedNode == null)
            {
                FetchSerializedProperty();
                if (_serializedNode == null)
                    return default;
            }
            
            var prop = _serializedNode.FindPropertyRelative(fieldInfoName);
            var field = new PropertyField();
            field.bindingPath = prop.propertyPath;
            extensionContainer.Add(field);
            return field;
        }
    }
}