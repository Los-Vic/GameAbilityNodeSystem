using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace NodeSystem.Editor.Elements
{
    public class NodeSystemEditorNode:Node
    {
        public NodeSystemNode Node { get; private set; }
        public readonly List<Port> Ports = new();

        private SerializedObject _graphAssetObject;
        private SerializedProperty _serializedNode;
        
        public NodeSystemEditorNode(NodeSystemNode node, SerializedObject graphAssetObject)
        {
            _graphAssetObject = graphAssetObject;
            AddToClassList("node-graph-node");
            Node = node;

            var type = node.GetType();
            var att = type.GetCustomAttribute<NodeAttribute>();
            this.title = att.Title;

            var depths = att.MenuItem.Split('/');
            foreach (var depth in depths)
            {
                AddToClassList(depth.ToLower().Replace(' ', '-'));
            }
            
            this.name = type.Name;
            foreach (var fieldInfo in type.GetFields())
            {
                var portAttribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                if (portAttribute != null)
                {
                    CreatePort(fieldInfo, portAttribute);
                }
                
                var exposedPropAttribute = fieldInfo.GetCustomAttribute<ExposedPropAttribute>();
                if (exposedPropAttribute != null)
                {
                    var propertyField = DrawField(fieldInfo.Name);
                    propertyField?.RegisterValueChangeCallback(OnFieldChangeCallback);
                }
            }
            
            RefreshExpandedState();
        }

        private void CreatePort(FieldInfo fieldInfo, PortAttribute portAttribute)
        {
            var port = InstantiatePort(portAttribute.Orientation, portAttribute.PortDirection, portAttribute.PortCapacity,
                fieldInfo.FieldType);

            port.portName = fieldInfo.Name;
            Ports.Add(port);

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