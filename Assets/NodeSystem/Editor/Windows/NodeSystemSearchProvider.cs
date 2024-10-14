using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NodeSystem.Editor.Windows
{
    public struct SearchContextElement
    {
        public Type TargetType { get; private set; }
        public string Title { get; private set; }

        public SearchContextElement(Type targetType, string title)
        {
            TargetType = targetType;
            Title = title;
        }
    }
    
    public class NodeSystemSearchProvider:ScriptableObject, ISearchWindowProvider
    {
        public NodeSystemGraphView GraphView;
        public VisualElement Target;
        private static List<SearchContextElement> _elements;
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Nodes"), 0));

            _elements = new List<SearchContextElement>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute(typeof(NodeAttribute));
                    if (attribute == null)
                        continue;
                    var att = (NodeAttribute)attribute;
                    if (string.IsNullOrEmpty(att.MenuItem))
                        continue;
                    _elements.Add(new SearchContextElement(type, att.MenuItem));
                }
            }
            
            //Sort By Name
            _elements.Sort((entry1, entry2) =>
            {
                var splits1 = entry1.Title.Split('/');
                var splits2 = entry2.Title.Split('/');

                for (var i = 0; i < splits1.Length; ++i)
                {
                    if (i > splits2.Length)
                    {
                        return 1;
                    }

                    var value = string.Compare(splits1[i], splits2[i], StringComparison.Ordinal);
                    if (value == 0) 
                        continue;
                    if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
                        return splits1.Length < splits2.Length ? 1 : -1;
                    return value;
                }

                return 0;
            });

            //Create Tree
            var groups = new List<string>();
            foreach (var element in _elements)
            {
                var entryTitle = element.Title.Split("/");
                var groupName = "";
                for (var i = 0; i < entryTitle.Length - 1; ++i)
                {
                    groupName += entryTitle[i];
                    if (!groups.Contains(groupName))
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupName += "/";
                }

                var entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()))
                {
                    level = entryTitle.Length,
                    userData = element
                };
                tree.Add(entry);
            }
            
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition =
                GraphView.ChangeCoordinatesTo(GraphView, context.screenMousePosition - GraphView.Window.position.position);
            var graphMousePosition = GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var element = (SearchContextElement)searchTreeEntry.userData;
            if(!element.TargetType.IsSubclassOf(typeof(NodeSystemNode)))
                return false;
                
            var node = (NodeSystemNode)Activator.CreateInstance(element.TargetType);
            node.Position = new Rect(graphMousePosition, Vector2.one); 
            GraphView.AddNodeToGraphAsset(node);
            
            return true;
        }
    }
}