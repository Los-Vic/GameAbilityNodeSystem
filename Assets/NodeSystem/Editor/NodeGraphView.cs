using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = NS.Node;
using Random = UnityEngine.Random;

namespace NSEditor
{
    public class NodeGraphView:GraphView
    {
        private readonly SerializedObject _serializedObject;
        public readonly NodeGraphAsset GraphAsset;
        
        public NodeEditorWindow Window { get; private set; }
        private readonly List<EditorNode> _graphEditorNodes = new();
        private readonly Dictionary<string, EditorNode> _editorNodesMap = new();
        private readonly Dictionary<Edge, (NodePort, NodePort)> _edgeConnectionMap = new();
        private NodeSearchProvider _searchProvider;
        
        public NodeGraphView(SerializedObject serializedObject, NodeEditorWindow window)
        {
            _serializedObject = serializedObject;
            GraphAsset = (NodeGraphAsset)serializedObject.targetObject;
            Window = window;
            _searchProvider = window.CreateSearchProvider();
            _searchProvider.GraphView = this;
            
            //Add Style Sheet
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeSystem/Editor/GraphViewSs.uss");
            styleSheets.Add(styleSheet);
            
            //Add Grid Background
            var background = new GridBackground
            {
                name = "Grid"
            };
            Add(background);
            background.SendToBack();
            
            //Manipulator
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            
            //Add Node To Graph View
            ReDrawGraph();
            
            //Hook Callback
            nodeCreationRequest = ShowSearchWindow;
            graphViewChanged += OnGraphViewChanged;
            Undo.undoRedoEvent += OnUndoRedo;

            //Support Copy/Paste
            serializeGraphElements += OnSerializeGraphElements;
            canPasteSerializedData += data => true;
            unserializeAndPaste += OnUnserializeAndPaste;
        }

        [Serializable]
        private class CopyContent
        {
            [SerializeReference]
            public List<Node> Nodes = new();
        }
        
        private void OnUnserializeAndPaste(string operationname, string data)
        {
            Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[FlowGraph] Paste Nodes");
            var content = JsonUtility.FromJson<CopyContent>(data);
            var portIdMap = new Dictionary<string, string>();
            var newNodeList = new List<Node>();
            var newPortList = new List<NodePort>();

            var randomOffset = 100 * (Random.value * 2 - 1);
            //Create new nodes & ports
            foreach (var node in content.Nodes)
            {
                var type = node.GetType();
                var newNode = (Node)Activator.CreateInstance(type);
                newNode.Position = new Rect(node.Position.x + 200 + randomOffset, node.Position.y + 200 + randomOffset, node.Position.width,
                    node.Position.height);
                newNodeList.Add(newNode);
                
                foreach (var fieldInfo in type.GetFields())
                {
                    //copy field value
                    fieldInfo.SetValue(newNode, fieldInfo.GetValue(node));
                    
                    var attribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                    if(attribute == null)
                        continue;
                    var portId = (string)fieldInfo.GetValue(node);
                    var port = GraphAsset.GetPort(portId);

                    var newPort = new NodePort(newNode.Id, port.direction, attribute.PortType, attribute.IsFlowPort, port.connectPortId);
                    portIdMap.Add(port.Id, newPort.Id);
                    newPortList.Add(newPort);
                }
            }
            
            //Assign new portId
            foreach (var node in newNodeList)
            {
                var type = node.GetType();
                foreach (var fieldInfo in type.GetFields())
                {
                    var attribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                    if(attribute == null)
                        continue;
                    var portId = (string)fieldInfo.GetValue(node);
                    var newPortId = portIdMap[portId];
                    fieldInfo.SetValue(node, newPortId);
                }
            }
            
            foreach (var port in newPortList)
            {
                if(port.connectPortId == null)
                    continue;
                port.connectPortId = portIdMap.GetValueOrDefault(port.connectPortId);
            }
            
            //Add new nodes & ports
            foreach (var node in newNodeList)
            {
                GraphAsset.AddNode(node, false);
            }

            foreach (var port in newPortList)
            {
                GraphAsset.AddPort(port);
            }
            
            ReDrawGraph();
        }

        //Only Support Copy Nodes And ExposedProp Fields
        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var content = new CopyContent();
            foreach (var elem in elements)
            {
                if (elem is not EditorNode editorNode) 
                    continue;
                var node = editorNode.Node;
                var type = node.GetType();

                var nodeAttribute = type.GetCustomAttribute<NodeAttribute>();
                if (nodeAttribute is { IsSingleton: true })
                {
                    Debug.LogWarning($"Can't copy SingletonNode [{node.nodeName}]");
                    continue;
                }
                content.Nodes.Add(node);
            }

            var js = JsonUtility.ToJson(content);
            return js;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var allPort = new List<Port>();
            var portList = new List<Port>();

            foreach (var node in _graphEditorNodes)
            {
                allPort.AddRange(node.ViewPortToNodePort.Keys);
            }

            foreach (var port in allPort)
            {
                if(port == startPort) continue;
                if(port.node == startPort.node) continue;
                if(port.direction == startPort.direction) continue;
                if (port.portType.IsAssignableFrom(startPort.portType) 
                    || startPort.portType.IsAssignableFrom(port.portType))
                {
                    portList.Add(port);
                }
            }
            return portList;
        }

        #region Callbacks

        private void OnUndoRedo(in UndoRedoInfo undo)
        {
            if (!undo.undoName.Contains("[NodeSystem]"))
                return;

            _serializedObject.Update();
            ReDrawGraph();
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            //Move
            if (graphViewChange.movedElements != null)
            {
                Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[NodeSystem]Moved Elements");
                foreach (var editorNode in graphViewChange.movedElements.OfType<EditorNode>())
                {
                    editorNode.SavePosition();
                }
                SaveAsset();
            }
            
            //Delete
            if (graphViewChange.elementsToRemove != null)
            {
                Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[NodeSystem]Deleted Elements");
                foreach (var editorNode in graphViewChange.elementsToRemove.OfType<EditorNode>())
                {
                    RemoveNodeFromGraphAsset(editorNode);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>())
                {
                    RemoveConnectionFromGraphAsset(edge);
                    RefreshRerouteNode(edge, false);
                }
                SaveAsset();
            }
            
            //Add Edges
            if (graphViewChange.edgesToCreate != null)
            {
                Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[NodeSystem]Add Edges");
                foreach (var e in graphViewChange.edgesToCreate)
                {
                    AddConnectionToGraphAsset(e);
                    RefreshRerouteNode(e, true);
                }
                SaveAsset();
            }
            
            return graphViewChange;
        }
        

        /// <summary>
        /// Open Search Window
        /// </summary>
        /// <param name="obj"></param>
        private void ShowSearchWindow(NodeCreationContext obj)
        {
            _searchProvider.Target = (VisualElement)focusController.focusedElement;
            SearchWindow.Open(new SearchWindowContext(obj.screenMousePosition), _searchProvider);
        }

        #endregion
        

        /// <summary>
        /// Add Node to NodeGraphAsset
        /// </summary>
        /// <param name="node"></param>
        public void AddNodeToGraphAsset(Node node)
        {
            Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[FlowGraph] Add Node");
            GraphAsset.AddNode(node);
            _serializedObject.Update();
            AddNodeToGraphView(node);

            SaveAsset();
        }

        /// <summary>
        /// Add Node to GraphView
        /// </summary>
        /// <param name="node"></param>
        private void AddNodeToGraphView(Node node)
        {
            var editorNode = Window.CreateEditorNode();
            editorNode.Draw(node, _serializedObject);
            editorNode.SetPosition(node.Position);
            _graphEditorNodes.Add(editorNode);
            _editorNodesMap.Add(node.Id, editorNode);
            AddElement(editorNode);
            
            BindObject();
        }
        
        /// <summary>
        /// Remove Node
        /// </summary>
        private void RemoveNodeFromGraphAsset(EditorNode editorNode)
        {
            GraphAsset.RemoveNode(editorNode.Node);
            _editorNodesMap.Remove(editorNode.Node.Id);
            _graphEditorNodes.Remove(editorNode);
            _serializedObject.Update();
        }

        /// <summary>
        /// Add Connection to Graph Asset
        /// </summary>
        /// <param name="edge"></param>
        private void AddConnectionToGraphAsset(Edge edge)
        {
            var inNode = (EditorNode)edge.input.node;
            var inPortId = inNode.ViewPortToNodePort[edge.input];
            
            var outNode = (EditorNode)edge.output.node;
            var outPortId = outNode.ViewPortToNodePort[edge.output];

            var inPort = GraphAsset.GetPort(inPortId);
            var outPort = GraphAsset.GetPort(outPortId);
            inPort.ConnectTo(outPortId);
            outPort.ConnectTo(inPortId);
            
            _edgeConnectionMap.Add(edge, (inPort, outPort));
        }

        /// <summary>
        /// Remove Connection from Graph Asset
        /// </summary>
        /// <param name="edge"></param>
        private void RemoveConnectionFromGraphAsset(Edge edge)
        {
            if (_edgeConnectionMap.Remove(edge, out var connection))
            {
                connection.Item1.Disconnect();
                connection.Item2.Disconnect();
            }
        }
        
        public void ReDrawGraph()
        {
            GraphAsset.LoadMap();
            
            foreach (var element in graphElements)
            {
                RemoveElement(element);
            }
            _graphEditorNodes.Clear();
            _editorNodesMap.Clear();
            foreach (var node in GraphAsset.nodes)
            {
                AddNodeToGraphView(node);
            }

            //Create Edge By Input Port
            foreach (var editorNode in _editorNodesMap.Values)
            {
                foreach (var (nodePortId, viewPort) in editorNode.NodePortToViewPort)
                {
                    var nodePort = GraphAsset.GetPort(nodePortId);
                    if(nodePort.direction == Direction.Output)
                        continue;
                    
                    if(string.IsNullOrEmpty(nodePort.connectPortId))
                        continue;

                    var outNodePort = GraphAsset.GetPort(nodePort.connectPortId);
                    var outEditorNode = _editorNodesMap[outNodePort.belongNodeId];
                    var outViewPort = outEditorNode.NodePortToViewPort[outNodePort.Id];
                    
                    var edge = viewPort.ConnectTo(outViewPort);
                    RefreshRerouteNode(edge, true);
                    AddElement(edge);
                    _edgeConnectionMap.Add(edge, (nodePort, outNodePort));
                }
            }
        }

        private void SaveAsset()
        {
            EditorUtility.SetDirty(GraphAsset);
            AssetDatabase.SaveAssetIfDirty(GraphAsset);
        }

        private void BindObject()
        {
            _serializedObject.Update();
            this.Bind(_serializedObject);
        }

        private void RefreshRerouteNode(Edge edge, bool isAdd)
        {
            var inNode = edge.input.node as EditorNode;
            var outNode = edge.output.node as EditorNode;
            inNode?.RefreshRerouteNode(edge, isAdd);
            outNode?.RefreshRerouteNode(edge, isAdd);
        }
    }
}