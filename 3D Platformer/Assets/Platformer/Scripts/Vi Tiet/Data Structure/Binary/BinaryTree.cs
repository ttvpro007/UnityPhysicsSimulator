using System;
using System.Collections.Generic;

namespace ViTiet.DataStructure.Binary
{
    public class BinaryTree<T> where T : IComparable<T>
    {
        private Node<T> root = null;
        private Dictionary<Node<T>, KeyValuePair<int, string>> nodes = null;
        private Dictionary<Node<T>, KeyValuePair<int, string>> lefts = null;
        private Dictionary<Node<T>, KeyValuePair<int, string>> rights = null;
        public Dictionary<Node<T>, KeyValuePair<int, string>> Nodes { get { return nodes; } }

        public BinaryTree()
        {
            nodes = new Dictionary<Node<T>, KeyValuePair<int, string>>();
            lefts = new Dictionary<Node<T>, KeyValuePair<int, string>>();
            rights = new Dictionary<Node<T>, KeyValuePair<int, string>>();
        }

        public BinaryTree(Node<T> root)
        {
            this.root = root;
            nodes = new Dictionary<Node<T>, KeyValuePair<int, string>>();
            lefts = new Dictionary<Node<T>, KeyValuePair<int, string>>();
            rights = new Dictionary<Node<T>, KeyValuePair<int, string>>();

        }

        public int Count { get { return nodes.Count; } }

        public void Add(T data)
        {
            int depth = 0;
            Node<T> node = new Node<T>(data);

            if (root == null)
            {
                node.ID = IDGenerator.GenerateRootID();
                root = node;
                nodes.Add(node, new KeyValuePair<int, string>(depth, "Root"));
                return;
            }

            if (!IsNodeDuplicated(root, node))
                AddRecursive(depth + 1, root, node);
        }

        private void AddRecursive(int depth, Node<T> parent, Node<T> node)
        {
            // check if current data is less than root data
            if (IsNodeLeftSide(parent, node))
            {
                if (parent.Left == null)
                {
                    node.ID = IDGenerator.GenerateID(nodes.Count, depth, true);
                    node.Parent = parent;
                    parent.Left = node;
                    KeyValuePair<int, string> pair = new KeyValuePair<int, string>(depth, "Left");
                    nodes.Add(node, pair);
                    lefts.Add(node, pair);
                }
                else
                {
                    if (!IsNodeDuplicated(parent, node))
                        AddRecursive(depth + 1, parent.Left, node);
                }
            }
            else
            {
                if (parent.Data.Equals(node.Data)) return;
                if (parent.Right == null)
                {
                    node.ID = IDGenerator.GenerateID(nodes.Count, depth, false);
                    node.Parent = parent;
                    parent.Right = node;
                    KeyValuePair<int, string> pair = new KeyValuePair<int, string>(depth, "Right");
                    nodes.Add(node, pair);
                    rights.Add(node, pair);
                }
                else
                {
                    if (!IsNodeDuplicated(parent, node))
                        AddRecursive(depth + 1, parent.Right, node);
                }
            }
        }

        public void Remove(T data)
        {

        }

        private bool IsNodeLeftSide(Node<T> parent, Node<T> node)
        {
            return node.Data.CompareTo(parent.Data) == -1;
        }

        private bool IsNodeDuplicated(Node<T> parent, Node<T> node)
        {
            return node.Data.Equals(parent.Data);
        }
    }
}