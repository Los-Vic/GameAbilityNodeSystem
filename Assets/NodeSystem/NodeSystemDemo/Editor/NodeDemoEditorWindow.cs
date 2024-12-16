using NSEditor;
using UnityEditor;
using UnityEngine;

namespace NS
{
    public class NodeDemoEditorWindow:NodeEditorWindow
    {
        public override EditorNode CreateEditorNode()
        {
            return new NodeSystemDemoEditorNode();
        }
    }
}