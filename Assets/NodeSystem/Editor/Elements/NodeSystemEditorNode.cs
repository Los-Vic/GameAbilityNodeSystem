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
            if (att.InPortNums > 0)
            {
                CreateInputPorts(att.InPortNums);
            }

            if (att.OutPortNums > 0)
            {
                CreateOutPorts(att.OutPortNums);
            }

            foreach (var fieldInfo in type.GetFields())
            {
                var attr = fieldInfo.GetCustomAttribute<ExposedPropAttribute>();
                if (attr == null)
                    continue;

                var propertyField = DrawField(fieldInfo.Name);
                propertyField?.RegisterValueChangeCallback(OnFieldChangeCallback);
            }
            
            RefreshExpandedState();
        }

        private void OnFieldChangeCallback(SerializedPropertyChangeEvent evt)
        {
            
        }

        public void SavePosition()
        {
            Node.Position = GetPosition();
        }

        private void CreateInputPorts(uint portNums)
        {
            for (var i = 0; i < portNums; ++i)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));

                port.portName = i == 0 ? "In" : $"In{i}";
                Ports.Add(port);
                inputContainer.Add(port);
            }
        }

        private void CreateOutPorts(uint portNums)
        {
            for (var i = 0; i < portNums; ++i)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(bool));

                port.portName = i == 0 ? "Out" : $"Out{i}";
                Ports.Add(port);
                outputContainer.Add(port);
            }
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