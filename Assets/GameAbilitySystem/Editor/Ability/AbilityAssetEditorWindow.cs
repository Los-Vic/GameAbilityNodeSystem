using System.Collections.Generic;
using GAS.Logic;
using NSEditor;

namespace GAS.Editor
{
    public class AbilityAssetEditorWindow:NodeEditorWindow
    {
        public override NodeSearchProvider CreateSearchProvider()
        {
            var provider = CreateInstance<NodeSearchProvider>();
            provider.SetScopeList(new List<int>()
            {
                0, NodeScopeDefine.System, NodeScopeDefine.Ability
            });

            return provider;
        }

        public override EditorNode CreateEditorNode()
        {
            return new SystemEditorNode();
        }
    }
}