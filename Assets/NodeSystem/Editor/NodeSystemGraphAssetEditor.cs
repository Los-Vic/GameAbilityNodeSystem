using System.Collections.Generic;
using System.Reflection;
using NS;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NSEditor
{
    [CustomEditor(typeof(NodeGraphAsset), true)]
    public class NodeSystemGraphAssetEditor:Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            var type = asset.GetType();
            if (!typeof(NodeGraphAsset).IsAssignableFrom(type)) 
                return false;
            NodeEditorWindow.Open<NodeSystemEditorWindow>((NodeGraphAsset)asset);
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            //Open Graph Button
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("OpenGraph"))
            {
                NodeEditorWindow.Open<NodeSystemEditorWindow>((NodeGraphAsset)target);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("ValidateGraph"))
            {
                NodeGraphAssetEditorUtility.ValidateGraph(serializedObject);
            }
            GUI.backgroundColor = oldColor;
        }
        
    }

    public static class NodeGraphAssetEditorUtility
    {
        public static void ValidateGraph(SerializedObject serializedObject)
        {
            var graphAsset = serializedObject.targetObject as NodeGraphAsset;
            if(graphAsset == null)
                return;
            
            var nodeMap = new Dictionary<string, Node>();
            var portMap = new Dictionary<string, NodePort>();
            var portListMap = new Dictionary<string, List<NodePort>>();
            var badPorts = new HashSet<NodePort>();
            var badPortIds = new HashSet<string>();
            var badNodes = new HashSet<Node>();
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
                    //Break Connection
                    port.Disconnect();
                }
            }
            //Remove Bad Nodes & Ports
            foreach (var node in badNodes)
            {
                graphAsset.nodes.Remove(node);
            }
            foreach (var port in badPorts)
            {
                graphAsset.ports.Remove(port);
            }
        }
    }
}