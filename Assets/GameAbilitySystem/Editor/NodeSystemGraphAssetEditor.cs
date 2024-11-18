using System.Collections.Generic;
using System.Reflection;
using GAS.Logic;
using NS;
using NSEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GAS.Editor
{
    [CustomEditor(typeof(AbilityAsset), true)]
    public class AbilityAssetEditor:OdinEditor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset.GetType() != typeof(AbilityAsset)) 
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
            var portMap = new Dictionary<string, NodeSystemPort>();
            var portListMap = new Dictionary<string, List<NodeSystemPort>>();
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
                    portList = new List<NodeSystemPort>();
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