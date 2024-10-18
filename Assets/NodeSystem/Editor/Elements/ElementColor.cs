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
            { typeof(FlowPort), new Color(0.8f, 0, 0, 1) },
        };
        
        
        public static Color GetPortColor(Type nodeType)
        {
            return PortColorMap.TryGetValue(nodeType, out var color) ? color : Color.white;
        }

        public static Color GetNodeColor(ENodeCategory nodeCategory)
        {
            switch (nodeCategory)
            {
                case ENodeCategory.Flow:
                    return new Color(0.5f, 0, 0, 1);
                case ENodeCategory.Value:
                    return new Color(0, 0.5f, 0, 1);
                case ENodeCategory.Start:
                    return new Color(0.6f, 0, 0, 1);
            }
            return Color.magenta;
        }
    }
}