using System;
using System.Collections.Generic;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;
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
        private NodeGraphAsset _graphAsset;

        private static readonly Color DefaultPortColor = new Color(0, 0.8f, 0.8f, 1);
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(int), new Color(0, 0.8f, 0, 1) },
            { typeof(float), new Color(0f, 0.6f, 0.5f, 1)},
            { typeof(bool), new Color(0.8f, 0f, 0f, 1) },
            { typeof(BaseFlowPort), new Color(0.8f, 0.8f, 0.8f, 1) },
            { typeof(string), new Color(0.8f,0,0.8f)},
            { typeof(object), new Color(0.3f, 0.3f, 0.3f, 1)}
        };
        
        public virtual void Draw(Node node, SerializedObject graphAssetObject)
        {
            _graphAssetObject = graphAssetObject;
            _graphAsset = _graphAssetObject.targetObject as NodeGraphAsset;
            
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
                    CreatePortalTypeExtension(fieldInfo);
                }

                //Create Extensions
                var exposedPropAttribute = fieldInfo.GetCustomAttribute<ExposedPropAttribute>();
                if (exposedPropAttribute != null)
                {
                    DrawField(fieldInfo);
                }
            }
            RefreshExpandedState();
            CheckPortalNodeIsSingle();
        }

        public void RefreshRerouteNode(Edge changeEdge, bool isAdd)
        {
            if(!Node.IsRerouteNode())
                return;
            var connections = new List<Edge>();
            foreach (var port in NodePortToViewPort.Values)
            {
                connections.AddRange(port.connections);
            }

            if (isAdd)
            {
                connections.Add(changeEdge);
            }
            else
            {
                connections.Remove(changeEdge);
            }
            
            Type portType = null;
            foreach (var edge in connections)
            {
                portType = edge.input.node != this ? edge.input.portType : edge.output.portType;
            }
            foreach (var port in NodePortToViewPort.Values)
            {
                port.portType = portType ?? typeof(object);
                port.portColor = GetPortColor(port.portType);
                port.portName = "";
                port.tooltip = port.portType.Name;
            }
        }

        protected virtual Color GetNodeColor(int nodeCategory)
        {
            switch (nodeCategory)
            {
                case CommonNodeCategory.Action:
                case CommonNodeCategory.Task:
                    return new Color(0, 0.3f, 0.7f, 1);
                case CommonNodeCategory.Debug:
                    return new Color(0.7f, 0.3f, 0.7f, 1);
                case CommonNodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case CommonNodeCategory.Portal:
                    return new Color(0.6f, 0, 0, 1);
                case CommonNodeCategory.FlowControl:
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
            if (Node.IsRerouteNode())
            {
                titleContainer.RemoveFromHierarchy();
                return;
            }
            
            title = attribute.Title;
            if (attribute.NodeCategory == CommonNodeCategory.Task)
            {
                var img = new Image
                {
                    image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/NodeSystem/Editor/Icon/clock.png"),
                    style = { width = 28, paddingRight = 6 }
                };
                titleButtonContainer.Add(img);
            }
            else if (attribute.NodeCategory == CommonNodeCategory.Debug)
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
            var port = InstantiatePort(Orientation.Horizontal, GetPortDirection(portAttribute.PortDirection),
                Port.Capacity.Single,
                portAttribute.PortType);
            
            port.portName = portAttribute.PortName;
            port.portColor = GetPortColor(portAttribute.PortType);
            port.tooltip = portAttribute.PortType.Name;
            var nodePort = (string)fieldInfo.GetValue(Node);
            ViewPortToNodePort.Add(port, nodePort);
            NodePortToViewPort.Add(nodePort, port);

            if (portAttribute.PortDirection == EPortDirection.Input)
            {
                inputContainer.Add(port);
            }
            else
            {
                outputContainer.Add(port);
            }
        }

        private void CreatePortalTypeExtension(FieldInfo fieldInfo)
        {
            var propertyField = DrawField(fieldInfo);
            propertyField?.RegisterValueChangeCallback(OnPortalTypeFieldChangeCallback);
            var val = fieldInfo.GetValue(Node);
            var newTitle = val.ToString();
            title = newTitle;
            Node.SetNodeName(newTitle);
        }

        private void OnPortalTypeFieldChangeCallback(SerializedPropertyChangeEvent evt)
        {
            var newTitle = evt.changedProperty.enumDisplayNames[evt.changedProperty.enumValueIndex];
            title = newTitle;
            Node.SetNodeName(newTitle);
            CheckPortalNodeIsSingle();
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

        private PropertyField DrawField(FieldInfo fieldInfo)
        {
            if (_serializedNode == null)
            {
                FetchSerializedProperty();
                if (_serializedNode == null)
                    return default;
            }
            
            var prop = _serializedNode.FindPropertyRelative(fieldInfo.Name);
            var field = new PropertyField(prop);
            
            //create dropdown field to select instance
            if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
            {
                var baseType = fieldInfo.FieldType;
                var typeList = GetSerializeReferenceChildClasses(baseType);
                
                var dropdownField = new DropdownField("Class");
                dropdownField.choices.Add("Null");
                foreach (var t in typeList)
                {
                    dropdownField.choices.Add(t.Name);
                }

                if (prop.managedReferenceValue == null)
                {
                    dropdownField.index = 0;
                }
                else
                {
                    var t = prop.managedReferenceValue.GetType();
                    for (var i = 0; i < dropdownField.choices.Count; i++)
                    {
                        if (dropdownField.choices[i] == t.Name)
                        {
                            dropdownField.index = i;
                            break;
                        }
                    }
                }

                dropdownField.RegisterValueChangedCallback((evt) => 
                    OnSerializeReferenceDropdownFieldValueChanged(evt, prop, typeList, field));
                extensionContainer.Add(dropdownField);
            }
            
           
            field.BindProperty(prop);
           // field.bindingPath = prop.propertyPath;
            extensionContainer.Add(field);
            
            return field;
        }

        private void OnSerializeReferenceDropdownFieldValueChanged(ChangeEvent<string> evt, SerializedProperty property, 
            List<Type> typeList, PropertyField propertyField)
        {
            if (evt.newValue == "Null")
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }
            
            foreach (var t in typeList)
            {
                if (t.Name == evt.newValue)
                {
                    property.managedReferenceValue = Activator.CreateInstance(t);
                    property.serializedObject.ApplyModifiedProperties();

                    if (!propertyField.IsBound())
                    {
                        propertyField.BindProperty(property);
                    }
                    break;
                }
            }
        }

        private List<Type> GetSerializeReferenceChildClasses(Type baseType)
        {
            var list = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType))
                    {
                        list.Add(type);
                    }
                }
            }
            return list;
        }
        
        

        private Direction GetPortDirection(EPortDirection dir)
        {
            switch (dir)
            {
                case EPortDirection.Input:
                    return Direction.Input;
                case EPortDirection.Output:
                    return Direction.Output;
            }
            return Direction.Input;
        }

        private void CheckPortalNodeIsSingle()
        {
            if(!Node.IsPortalNode())
                return;

            if (_graphAsset.GetNodeNameCount(Node.NodeName) > 1)
            {
                NodeSystemLogger.LogError($"Portal node is repeated! {Node.NodeName}");
            }
        }
    }
}