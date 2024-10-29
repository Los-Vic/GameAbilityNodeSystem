﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeSystem.Core;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NodeSystem.Editor.Windows
{
    [CustomEditor(typeof(NodeSystemGraphAsset))]
    public class NodeSystemGraphAssetEditor:UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() != typeof(NodeSystemGraphAsset)) 
                return false;
            NodeSystemEditorWindow.Open((NodeSystemGraphAsset)asset);
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
                NodeSystemEditorWindow.Open((NodeSystemGraphAsset)target);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("ValidateGraph"))
            {
                ValidateGraph();
            }
            GUI.backgroundColor = oldColor;
        }

        private void ValidateGraph()
        {
            var graphAsset = serializedObject.targetObject as NodeSystemGraphAsset;
            var nodeMap = new Dictionary<string, NodeSystemNode>();
            var portMap = new Dictionary<string, List<NodeSystemPort>>();
            var badPorts = new HashSet<NodeSystemPort>();
            var badPortIds = new HashSet<string>();
            var badNodes = new HashSet<NodeSystemNode>();
            //Construct Node Map
            foreach (var node in graphAsset.nodes)
            {
                nodeMap.Add(node.Id, node);
            }
            //Construct Port Map
            foreach (var port in graphAsset.ports)
            {
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
                
                if (!portMap.TryGetValue(port.belongNodeId, out var portList))
                {
                    portList = new List<NodeSystemPort>();
                    portMap.Add(port.belongNodeId, portList);
                }
                
                portList.Add(port);
            }
            //Find Bad Nodes & Bad Ports
            foreach (var node in graphAsset.nodes)
            {
                if (!portMap.TryGetValue(node.Id, out var portList))
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
                    
                    var valid = false;
                    foreach (var port in portList)
                    {
                        if (port.portType != portAttribute.PortType.AssemblyQualifiedName ||
                            port.direction != portAttribute.PortDirection) 
                            continue;
                        
                        valid = true;
                        portAttributeCounter++;
                        break;
                    }

                    if (valid)
                        continue;
                    
                    break;
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
                if (badPortIds.Contains(port.connectPortId) || !portMap.ContainsKey(port.Id))
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