using System;
using System.Collections.Generic;
using NS;
using UnityEngine;

namespace NSEditor
{
    public static class ElementColor
    {
        private static readonly Color DefaultPortColor = new Color(0, 0.8f, 0.8f, 1);
        
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(int), new Color(0, 0.8f, 0, 1) },
            { typeof(float), new Color(0f, 0.6f, 0.5f, 1)},
            { typeof(bool), new Color(0.8f, 0f, 0f, 1) },
            { typeof(ExecutePort), new Color(0.8f, 0.8f, 0.8f, 1) },
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
                case ENodeCategory.Event:
                    return new Color(0.6f, 0, 0, 1);
                case ENodeCategory.FlowControl:
                    return new Color(0.5f,0.5f,0.5f,1);
            }
            return Color.magenta;
        }
    }
}