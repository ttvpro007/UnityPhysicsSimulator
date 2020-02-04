using System;

namespace ViTiet.DataStructure.Binary
{
    public class Node<T> where T : IComparable<T>
    {
        public string ID { get; set; } = "";
        public Node(T t) { Data = t; }
        public Node<T> Parent { get; set; } = null;
        public Node<T> Left { get; set; } = null;
        public Node<T> Right { get; set; } = null;
        public T Data { get; set; } = default;
        public bool Visited { get; set; } = false;

        public bool IsRootNode()
        {
            return Parent == null;
        }

        public bool IsInternalNode()
        {
            return Parent != null && (Left != null || Right != null);
        }

        public bool IsLeafNode()
        {
            return Parent != null && Left == null && Right == null;
        }
    }
}