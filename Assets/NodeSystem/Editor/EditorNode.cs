﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
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
            { typeof(object), new Color(0.3f, 0.3f, 0.3f, 1)},
            { typeof(IEnumerable), new Color(0.3f, 0.3f, 0.3f, 1)}
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

            tooltip = att.ToolTip;

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
                var portalTypeAttribute = fieldInfo.GetCustomAttribute<EntryAttribute>();
                if (portalTypeAttribute != null)
                {
                    CreatePortalTypeExtension(fieldInfo);
                }

                //Create Extensions
                var exposedPropAttribute = fieldInfo.GetCustomAttribute<ExposedAttribute>();
                if (exposedPropAttribute != null)
                {
                    DrawField(fieldInfo);
                }
            }
            RefreshExpandedState();
            CheckPortalNodeIsSingle();
        }

        public void RefreshDynamicPortNode(Edge changeEdge, bool isAdd)
        {
            //Reroute node
            if (Node is RerouteNode)
            {
                RefreshRerouteNode(changeEdge, isAdd);
            }
            else if (Node is ForEachNode forEachNode)
            {
                RefreshForeachNode(forEachNode, changeEdge, isAdd);
            }
        }

        private void RefreshRerouteNode(Edge changeEdge, bool isAdd)
        {
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
        
        private void RefreshForeachNode(ForEachNode forEachNode, Edge edge, bool isAdd)
        {
            Type portType = null;
            Port enumerablePort = null;
            if (ViewPortToNodePort.TryGetValue(edge.input, out var inPort) && inPort == forEachNode.InEnumerable)
            {
                var t = edge.output.portType;
                var args = t.GetGenericArguments();
                if (args is { Length: > 0 })
                {
                    portType = args[0];
                }

                enumerablePort = edge.input;
            }
            if (ViewPortToNodePort.TryGetValue(edge.output, out var outPort) && outPort == forEachNode.InEnumerable)
            {
                var t = edge.input.portType;
                var args = t.GetGenericArguments();
                if (args is { Length: > 0 })
                {
                    portType = args[0];
                }

                enumerablePort = edge.output;
            }

            if (portType == null)
                return;
            
            portType = isAdd ? portType : typeof(object);
            
            enumerablePort.portColor = GetPortColor(portType);

            if (!NodePortToViewPort.TryGetValue(forEachNode.OutElement, out var elemPort)) 
                return;
            elemPort.portType = portType;
            elemPort.portColor = GetPortColor(elemPort.portType);
            elemPort.portName = elemPort.portType.Name;
            elemPort.tooltip = elemPort.portType.Name;
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
                case CommonNodeCategory.Entry:
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
            if (Node is RerouteNode)
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
            field.BindProperty(prop);
            
            // field.bindingPath = prop.propertyPath;
            extensionContainer.Add(field);
            
            //create dropdown field to select instance
            if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
            {
                var baseType = fieldInfo.FieldType;
                var typeList = GetSerializeReferenceChildClasses(baseType);
                
                var dropdownField = new DropdownField("ClassType");
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
            if(!Node.IsEntryNode())
                return;

            if (_graphAsset.GetNodeNameCount(Node.NodeName) > 1)
            {
                Debug.LogError($"[Editor]Portal node is repeated! {Node.NodeName}");
            }
        }
    }
}