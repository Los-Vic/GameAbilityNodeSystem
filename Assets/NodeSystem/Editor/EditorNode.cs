using System;
using System.Collections.Generic;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = NS.Node;

namespace NSEditor
{
    /// <summary>
    /// 继承此类绘制新的节点
    /// </summary>
    public class EditorNode : UnityEditor.Experimental.GraphView.Node
    {
        public Node Node { get; private set; }
        public readonly Dictionary<Port, string> ViewPortToNodePort = new();
        public readonly Dictionary<string, Port> NodePortToViewPort = new();

        private SerializedObject _graphAssetObject;
        private SerializedProperty _serializedNode;

        public virtual void Draw(Node node, SerializedObject graphAssetObject)
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

                //Create EventType
                var eventTypeAttribute = fieldInfo.GetCustomAttribute<EventTypeAttribute>();
                if (eventTypeAttribute != null)
                {
                    CreateEventTypeExtension(fieldInfo, Node);
                }

                //Create Extensions
                var exposedPropAttribute = fieldInfo.GetCustomAttribute<ExposedPropAttribute>();
                if (exposedPropAttribute != null)
                {
                    DrawField(fieldInfo.Name);
                }
            }
            RefreshExpandedState();
        }

        protected virtual Color GetNodeColor(int nodeCategory)
        {
            return Color.magenta;
        }

        protected virtual Color GetPortColor(Type type)
        {
            return Color.magenta;
        }
        
        protected virtual void ConstructTitle(Type type, NodeAttribute attribute)
        {
            title = attribute.Title;
            var box = new Box
            {
                style =
                {
                    backgroundColor = GetNodeColor(attribute.NodeCategory)
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
            var port = InstantiatePort(portAttribute.Orientation, portAttribute.PortDirection,
                portAttribute.PortCapacity,
                portAttribute.PortType);

            port.portName = portAttribute.PortName;
            port.portColor = GetPortColor(portAttribute.PortType);
            port.tooltip = portAttribute.PortType.Name;
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

        private void CreateEventTypeExtension(FieldInfo fieldInfo, Node node)
        {
            var propertyField = DrawField(fieldInfo.Name);
            propertyField?.RegisterValueChangeCallback(OnEventTypeFieldChangeCallback);
            var val = fieldInfo.GetValue(node);
            title = val.ToString();
        }

        private void OnEventTypeFieldChangeCallback(SerializedPropertyChangeEvent evt)
        {
            title = evt.changedProperty.enumDisplayNames[evt.changedProperty.enumValueIndex];
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