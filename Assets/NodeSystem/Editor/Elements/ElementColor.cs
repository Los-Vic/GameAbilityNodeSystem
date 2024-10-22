using System;
using System.Collections.Generic;
using NodeSystem.Ports;
using UnityEngine;

namespace NodeSystem.Editor.Elements
{
    public static class ElementColor
    {
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(IntPort), new Color(0, 0.8f, 0, 1) },
            { typeof(FlowPort), new Color(0.8f, 0.8f, 0.8f, 1) },
        };
        
        
        public static Color GetPortColor(Type nodeType)
        {
            return PortColorMap.TryGetValue(nodeType, out var color) ? color : Color.white;
        }

        public static Color GetNodeColor(ENodeCategory nodeCategory)
        {
            switch (nodeCategory)
            {
                case ENodeCategory.FlowInstant:
                case ENodeCategory.FlowNonInstant:
                case ENodeCategory.DebugFlowInstant:
                    return new Color(0.5f, 0, 0, 1);
                case ENodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case ENodeCategory.Start:
                    return new Color(0.6f, 0.2f, 0, 1);
                case ENodeCategory.Event:
                    return new Color(0.6f, 0.4f, 0, 1);
            }
            return Color.magenta;
        }
    }
}