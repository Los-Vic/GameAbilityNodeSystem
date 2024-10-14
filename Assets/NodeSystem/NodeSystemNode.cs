using System;
using UnityEngine;

namespace NodeSystem
{
    [Serializable]
    public class NodeSystemNode
    {
        [SerializeField] private string guid = Guid.NewGuid().ToString();
        [SerializeField] private Rect position;

        public string typeName;
        public string Id => guid;

        public Rect Position
        {
            get => position;
            set => position = value;
        }
    }
}