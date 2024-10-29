using System;
using System.Collections.Generic;
using NodeSystem.Core;
using NodeSystem.Ports;
using UnityEngine;

namespace NodeSystem.Editor.Elements
{
    public static class ElementColor
    {
        private static readonly Color DefaultPortColor = new Color(0, 0.8f, 0.8f, 1);
        
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(int), new Color(0, 0.8f, 0, 1) },
            { typeof(float), new Color(0f, 0.6f, 0.5f, 1)},
            { typeof(FlowPort), new Color(0.8f, 0.8f, 0.8f, 1) },
        };
        
        
        public static Color GetPortColor(Type nodeType)
        {
            return PortColorMap.GetValueOrDefault(nodeType, DefaultPortColor);
        }

        public static Color GetNodeColor(ENodeCategory nodeCategory)
        {
            switch (nodeCategory)
            {
                case ENodeCategory.ExecInstant:
                case ENodeCategory.ExecNonInstant:
                case ENodeCategory.ExecDebugInstant:
                    return new Color(0, 0.3f, 0.7f, 1);
                case ENodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case ENodeCategory.Start:
                    return new Color(0.6f, 0, 0, 1);
                case ENodeCategory.Event:
                    return new Color(0.6f, 0, 0, 1);
            }
            return Color.magenta;
        }
    }
}