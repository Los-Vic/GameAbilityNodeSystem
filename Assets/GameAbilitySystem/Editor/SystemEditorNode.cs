using System;
using System.Collections.Generic;
using GAS.Logic;
using MissQ;
using NSEditor;
using UnityEngine;

namespace GAS.Editor
{
    public class SystemEditorNode : EditorNode
    {
        private static readonly Dictionary<Type, Color> PortColorMap = new()
        {
            { typeof(GameUnit), new Color(0.2f, 0.2f, 1f, 1) },
            { typeof(List<GameUnit>), new Color(0.2f, 0.2f, 1f, 1) },
            { typeof(GameAbility), new Color(1f, 0.2f, 0.2f, 1) },
            { typeof(GameEffect), new Color(0.2f, 1f, 0.2f, 1) },
            { typeof(FP), new Color(0.7f, 0.7f, 0, 1) }
        };

        protected override Color GetNodeColor(int nodeCategory)
        {
            switch (nodeCategory)
            {
                case NodeCategoryDefine.AbilityEntry:
                    return new Color(0.9f, 0.3f, 0.1f);
                case NodeCategoryDefine.EffectNode:
                    return new Color(0, 0.5f, 0.7f, 1);
            }
            return base.GetNodeColor(nodeCategory);
        }

        protected override Color GetPortColor(Type type)
        {
            if (PortColorMap.TryGetValue(type, out var color))
                return color;
            return base.GetPortColor(type);
        }
    }
}