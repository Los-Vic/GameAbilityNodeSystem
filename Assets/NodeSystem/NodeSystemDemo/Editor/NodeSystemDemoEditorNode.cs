using System;
using System.Collections.Generic;
using NS;
using NS.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NSEditor
{
    public class NodeSystemDemoEditorNode:EditorNode
    {
        private static readonly Color DefaultPortColor = new Color(0, 0.8f, 0.8f, 1);
        
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(int), new Color(0, 0.8f, 0, 1) },
            { typeof(float), new Color(0f, 0.6f, 0.5f, 1)},
            { typeof(bool), new Color(0.8f, 0f, 0f, 1) },
            { typeof(BaseFlowPort), new Color(0.8f, 0.8f, 0.8f, 1) },
        };

        protected override void ConstructTitle(Type type, NodeAttribute attribute)
        {
            title = attribute.Title;
            if (attribute.NodeCategory == (int)ENodeCategory.ExecNonInstant)
            {
                var img = new Image
                {
                    image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/NodeSystem/Editor/Icon/clock.png"),
                    style = { width = 28, paddingRight = 6 }
                };
                titleButtonContainer.Add(img);
            }
            else if (attribute.NodeCategory == (int)ENodeCategory.ExecDebugInstant)
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

        protected override Color GetNodeColor(int nodeCategory)
        {
            switch (nodeCategory)
            {
                case (int)ENodeCategory.ExecInstant:
                case (int)ENodeCategory.ExecNonInstant:
                case (int)ENodeCategory.ExecDebugInstant:
                    return new Color(0, 0.3f, 0.7f, 1);
                case (int)ENodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case (int)ENodeCategory.Event:
                    return new Color(0.6f, 0, 0, 1);
                case (int)ENodeCategory.FlowControl:
                    return new Color(0.5f,0.5f,0.5f,1);
            }
            return Color.magenta;
        }

        protected override Color GetPortColor(Type type)
        {
            return PortColorMap.GetValueOrDefault(type, DefaultPortColor);
        }
    }
}