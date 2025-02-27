using System.Collections.Generic;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEngine;

namespace NSEditor
{
    /// <summary>
    /// Graph asset editor
    /// 定义了新的node graph asset后，通常需要创建新的asset editor类。在这里Open editor window。无法通过继承复用。
    /// </summary>
    [CustomEditor(typeof(NodeGraphAsset))]
    public class NodeSystemGraphAssetEditor:Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            //Open Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph", GUILayout.Height(48)))
            {
                NodeEditorWindow.Open<NodeEditorWindow>((NodeGraphAsset)target);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("ValidateGraph", GUILayout.Height(24)))
            {
                NodeGraphAssetEditorUtility.ValidateGraph(serializedObject);
            }
            if (GUILayout.Button("ClearGraph", GUILayout.Height(24)))
            {
                NodeGraphAssetEditorUtility.ClearGraph(serializedObject);
            }
            GUI.backgroundColor = oldColor;
        }
        
    }

    public static class NodeGraphAssetEditorUtility
    {
        public static bool ValidateGraph(SerializedObject serializedObject)
        {
            var noErrorFound = true;
            
            var graphAsset = serializedObject.targetObject as NodeGraphAsset;
            if (graphAsset == null)
            {
                Debug.LogError($"[Editor]Validate graph [{serializedObject.targetObject.name}] failed: not node graph asset.");
                return false;
            }
            
            var nodeMap = new Dictionary<string, Node>();
            var portMap = new Dictionary<string, NodePort>();
            var portListMap = new Dictionary<string, List<NodePort>>();
            var badPorts = new HashSet<NodePort>();
            var badPortIds = new HashSet<string>();
            var badNodes = new HashSet<Node>();
            
            //Remove Null
            for (var i = graphAsset.nodes.Count - 1; i >= 0; i--)
            {
                if (graphAsset.nodes[i] == null)
                {
                    noErrorFound = false;
                    graphAsset.nodes.RemoveAt(i);
                }
            }
            
            //Construct Node Map
            foreach (var node in graphAsset.nodes)
            {
                nodeMap.Add(node.Id, node);
            }
            //Construct Port Map
            foreach (var port in graphAsset.ports)
            {
                portMap.Add(port.Id, port);
                
                if (string.IsNullOrEmpty(port.belongNodeId))
                {
                    badPorts.Add(port);
                    badPortIds.Add(port.Id);
                    continue;
                }

                if (!nodeMap.ContainsKey(port.belongNodeId))
                {
                    badPorts.Add(port);
                    badPortIds.Add(port.Id);
                    continue;
                }
                
                if (!portListMap.TryGetValue(port.belongNodeId, out var portList))
                {
                    portList = new List<NodePort>();
                    portListMap.Add(port.belongNodeId, portList);
                }
                
                portList.Add(port);
            }
            //Find Bad Nodes & Bad Ports
            foreach (var node in graphAsset.nodes)
            {
                if (!portListMap.TryGetValue(node.Id, out var portList))
                {
                    badNodes.Add(node);
                    continue;
                }

                var portAttributeCounter = 0;
                var isBadNode = false;
                var type = node.GetType();
                foreach (var fieldInfo in type.GetFields())
                {
                    //Check Ports
                    var portAttribute = fieldInfo.GetCustomAttribute<PortAttribute>();
                    if (portAttribute == null) 
                        continue;

                    portAttributeCounter++;
                    var portId = (string)fieldInfo.GetValue(node);
                    if (portMap.TryGetValue(portId, out var port))
                    {
                        if (port.portType != portAttribute.PortType.AssemblyQualifiedName ||
                            port.direction != portAttribute.PortDirection)
                        {
                           isBadNode = true;
                        }
                    }
                    else
                    {
                        isBadNode = true;
                    }
                }

                if (portAttributeCounter != portList.Count)
                {
                    isBadNode = true;
                }

                if (!isBadNode) 
                    continue;

                badNodes.Add(node);
                foreach (var port in portList)
                {
                    badPorts.Add(port);
                    badPortIds.Add(port.Id);
                }
            }
            //Remove Bad Ports Connection
            foreach (var port in graphAsset.ports)
            {
                if(badPorts.Contains(port))
                    continue;
                if(!port.IsConnected())
                    continue;
                if (badPortIds.Contains(port.connectPortId) || !portMap.ContainsKey(port.connectPortId))
                {
                    noErrorFound = false;
                    //Break Connection
                    port.Disconnect();
                }
            }

            if (badNodes.Count > 0 || badPorts.Count > 0)
                noErrorFound = false;
            
            //Remove Bad Nodes & Ports
            foreach (var node in badNodes)
            {
                graphAsset.nodes.Remove(node);
            }
            foreach (var port in badPorts)
            {
                graphAsset.ports.Remove(port);
            }

            if (noErrorFound)
            {
                Debug.Log($"[Editor]Validate graph [{serializedObject.targetObject.name}]: no error found.");
            }
            else
            {
                Debug.LogWarning($"[Editor]Validate graph [{serializedObject.targetObject.name}]: some error has been fixed, please open graph to check.");
            }
           
            return noErrorFound;
        }

        public static void ClearGraph(SerializedObject serializedObject)
        {
            Debug.Log($"[Editor]Clear graph [{serializedObject.targetObject.name}]");
            var graphAsset = serializedObject.targetObject as NodeGraphAsset;
            if(graphAsset == null)
                return;
            graphAsset.nodes.Clear();
            graphAsset.ports.Clear();
        }
    }
}