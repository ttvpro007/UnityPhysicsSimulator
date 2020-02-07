using System.Collections.Generic;

namespace ViTiet.DataStructure.Graph
{
    public class Node<T>
    {
        T m_data;
        List<Node<T>> m_neighbors = new List<Node<T>>();
        List<WeightedEdge<T>> m_edges = new List<WeightedEdge<T>>();
        bool m_visited = false;

        public Node(T data) { m_data = data; }
        public T data { get { return m_data; } }
        public List<Node<T>> neighbors { get { return m_neighbors; } }
        public List<WeightedEdge<T>> edges { get { return m_edges; } }
        public bool visited { get { return m_visited; } set { m_visited = value; } }

        public void AddEdge(WeightedEdge<T> edge)
        {
            m_edges.Add(edge);
        }

        public override string ToString()
        {
            return string.Format("{0}", m_data.ToString());
        }
    }

}