using System.Collections.Generic;
using NSEditor;

namespace NS
{
    public class NodeDemoEditorWindow:NodeEditorWindow
    {
        public override EditorNode CreateEditorNode()
        {
            return base.CreateEditorNode();
        }

        public override NodeSearchProvider CreateSearchProvider()
        {
            var provider = CreateInstance<NodeSearchProvider>();
            provider.SetScopeList(new List<int>(){0, -1});

            return provider;
        }
    }
}