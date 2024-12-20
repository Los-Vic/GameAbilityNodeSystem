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

        private static readonly Color DefaultPortColor = new Color(0, 0.8f, 0.8f, 1);
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(int), new Color(0, 0.8f, 0, 1) },
            { typeof(float), new Color(0f, 0.6f, 0.5f, 1)},
            { typeof(bool), new Color(0.8f, 0f, 0f, 1) },
            { typeof(BaseFlowPort), new Color(0.8f, 0.8f, 0.8f, 1) },
            { typeof(string), new Color(0.8f,0,0.8f)}
        };
        
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

                //Create PortalType
                var portalTypeAttribute = fieldInfo.GetCustomAttribute<PortalTypeAttribute>();
                if (portalTypeAttribute != null)
                {
                    CreatePortalTypeExtension(fieldInfo, Node);
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
            switch (nodeCategory)
            {
                case (int)ECommonNodeCategory.Action:
                case (int)ECommonNodeCategory.Task:
                    return new Color(0, 0.3f, 0.7f, 1);
                case (int)ECommonNodeCategory.Debug:
                    return new Color(0.7f, 0.3f, 0.7f, 1);
                case (int)ECommonNodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case (int)ECommonNodeCategory.Portal:
                    return new Color(0.6f, 0, 0, 1);
                case (int)ECommonNodeCategory.FlowControl:
                    return new Color(0.5f,0.5f,0.5f,1);
            }
            return Color.magenta;
        }

        protected virtual Color GetPortColor(Type type)
        {
            return PortColorMap.GetValueOrDefault(type, DefaultPortColor);
        }
        
        protected virtual void ConstructTitle(Type type, NodeAttribute attribute)
        {
            if (attribute.Title == "")
            {
                titleContainer.RemoveFromHierarchy();
                return;
            }
            
            title = attribute.Title;
            if (attribute.NodeCategory == (int)ECommonNodeCategory.Task)
            {
                var img = new Image
                {
                    image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/NodeSystem/Editor/Icon/clock.png"),
                    style = { width = 28, paddingRight = 6 }
                };
                titleButtonContainer.Add(img);
            }
            else if (attribute.NodeCategory == (int)ECommonNodeCategory.Debug)
            {
                var img = new Image
                {
                    image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/NodeSystem/Editor/Icon/debug.png"),
                    style = { width = 32, paddingRight = 4 }
                };
                titleButtonContainer.Add(img);
            }

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

        private void CreatePortalTypeExtension(FieldInfo fieldInfo, Node node)
        {
            var propertyField = DrawField(fieldInfo.Name);
            propertyField?.RegisterValueChangeCallback(OnPortalTypeFieldChangeCallback);
            var val = fieldInfo.GetValue(node);
            title = val.ToString();
        }

        private void OnPortalTypeFieldChangeCallback(SerializedPropertyChangeEvent evt)
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