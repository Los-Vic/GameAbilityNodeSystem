using System.Collections.Generic;
using System.Linq;
using NodeSystem.Editor.Elements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeSystem.Editor.Windows
{
    public class NodeSystemGraphView:GraphView
    {
        private readonly SerializedObject _serializedObject;
        private readonly NodeSystemGraphAsset _graphAsset;
        public NodeSystemEditorWindow Window { get; private set; }

        private readonly List<NodeSystemEditorNode> _graphEditorNodes = new();
        private readonly Dictionary<string, NodeSystemEditorNode> _editorNodesMap = new();
        private readonly Dictionary<Edge, (NodeSystemPort, NodeSystemPort)> _edgeConnectionMap = new();

        private readonly List<NodeSystemEditorNode> _copyEditorNodes = new();

        private NodeSystemSearchProvider _searchProvider;
        public NodeSystemGraphView(SerializedObject serializedObject, NodeSystemEditorWindow window)
        {
            _serializedObject = serializedObject;
            _graphAsset = (NodeSystemGraphAsset)serializedObject.targetObject;
            Window = window;
            _searchProvider = ScriptableObject.CreateInstance<NodeSystemSearchProvider>();
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
        
        private void OnUnserializeAndPaste(string operationname, string data)
        {
            Debug.Log(operationname + "  " + data);
        }

        //Only Support Copy Nodes And ExposedProp Fields
        private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            foreach (var elem in elements)
            {
                if (elem is NodeSystemEditorNode node)
                {
                    var js = JsonUtility.ToJson(node.Node);
                    Debug.Log(js);
                }
               
            }
            
            return "Copy";
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var allPort = new List<Port>();
            var ports = new List<Port>();

            foreach (var node in _graphEditorNodes)
            {
                allPort.AddRange(node.ViewPortToNodePort.Keys);
            }

            foreach (var port in allPort)
            {
                if(port == startPort) continue;
                if(port.node == startPort.node) continue;
                if(port.direction == startPort.direction) continue;
                if (port.portType == startPort.portType)
                {
                    ports.Add(port);
                }
            }
            return ports;
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
                foreach (var editorNode in graphViewChange.movedElements.OfType<NodeSystemEditorNode>())
                {
                    editorNode.SavePosition();
                }
                SaveAsset();
            }
            
            //Delete
            if (graphViewChange.elementsToRemove != null)
            {
                Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[NodeSystem]Deleted Elements");
                foreach (var editorNode in graphViewChange.elementsToRemove.OfType<NodeSystemEditorNode>())
                {
                    RemoveNodeFromGraphAsset(editorNode);
                }

                foreach (var edge in graphViewChange.elementsToRemove.OfType<Edge>())
                {
                    RemoveConnectionFromGraphAsset(edge);
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
        public void AddNodeToGraphAsset(NodeSystemNode node)
        {
            Undo.RegisterCompleteObjectUndo(_serializedObject.targetObject, "[FlowGraph] Add Node");
            _graphAsset.AddNode(node);
            _serializedObject.Update();
            AddNodeToGraphView(node);

            SaveAsset();
        }

        /// <summary>
        /// Add Node to GraphView
        /// </summary>
        /// <param name="node"></param>
        private void AddNodeToGraphView(NodeSystemNode node)
        {
            node.typeName = node.GetType().AssemblyQualifiedName;
            var editorNode = new NodeSystemEditorNode(node, _serializedObject);
            editorNode.SetPosition(node.Position);
            _graphEditorNodes.Add(editorNode);
            _editorNodesMap.Add(node.Id, editorNode);
            AddElement(editorNode);
            
            BindObject();
        }
        
        /// <summary>
        /// Remove Node
        /// </summary>
        private void RemoveNodeFromGraphAsset(NodeSystemEditorNode editorNode)
        {
            _graphAsset.RemoveNode(editorNode.Node);
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
            var inNode = (NodeSystemEditorNode)edge.input.node;
            var inPortId = inNode.ViewPortToNodePort[edge.input];
            
            var outNode = (NodeSystemEditorNode)edge.output.node;
            var outPortId = outNode.ViewPortToNodePort[edge.output];

            var inPort = _graphAsset.GetPort(inPortId);
            var outPort = _graphAsset.GetPort(outPortId);
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
        
        private void ReDrawGraph()
        {
            _graphAsset.LoadMap();
            
            foreach (var element in graphElements)
            {
                RemoveElement(element);
            }
            _graphEditorNodes.Clear();
            _editorNodesMap.Clear();
            foreach (var node in _graphAsset.nodes)
            {
                AddNodeToGraphView(node);
            }

            //Create Edge By Input Port
            foreach (var editorNode in _editorNodesMap.Values)
            {
                foreach (var (nodePortId, viewPort) in editorNode.NodePortToViewPort)
                {
                    var nodePort = _graphAsset.GetPort(nodePortId);
                    if(nodePort.direction == Direction.Output)
                        continue;
                    
                    if(string.IsNullOrEmpty(nodePort.connectPortId))
                        continue;

                    var outNodePort = _graphAsset.GetPort(nodePort.connectPortId);
                    var outEditorNode = _editorNodesMap[outNodePort.belongNodeId];
                    var outViewPort = outEditorNode.NodePortToViewPort[outNodePort.Id];
                    
                    var edge = viewPort.ConnectTo(outViewPort);
                    AddElement(edge);
                    _edgeConnectionMap.Add(edge, (nodePort, outNodePort));
                }
            }
        }

        private void SaveAsset()
        {
            EditorUtility.SetDirty(_graphAsset);
            AssetDatabase.SaveAssetIfDirty(_graphAsset);
        }

        private void BindObject()
        {
            _serializedObject.Update();
            this.Bind(_serializedObject);
        }
    }
}