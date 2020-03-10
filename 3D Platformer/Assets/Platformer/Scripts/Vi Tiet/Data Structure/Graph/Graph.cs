using System.Collections.Generic;
using UnityEngine;

namespace ViTiet.DataStructure.Graph
{
    public class Graph<T>
    {
        List<Node<T>> m_nodes = new List<Node<T>>();
        List<WeightedEdge<T>> m_edges = new List<WeightedEdge<T>>();

        public List<Node<T>> nodes { get { return m_nodes; } }

        public List<WeightedEdge<T>> edges { get { return m_edges; } }

        public int nodeCount { get { return m_nodes.Count; } }

        public int edgeCount { get { return m_edges.Count; } }

        public Node<T> AddNode(T data)
        {
            Node<T> node = new Node<T>(data);
            m_nodes.Add(node);
            return node;
        }

        public Node<T> FindNode(T data)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].data.Equals(data))
                {
                    return nodes[i];
                }
            }

            return null;
        }

        public void AddEdge(Node<T> node1, Node<T> node2)
        {
            if (node1 == null || node2 == null)
            {
                return;
            }

            node1.neighbors.Add(node2);
            node2.neighbors.Add(node1);
        }

        public void AddEdge(Node<T> home, Node<T> neighbor, float weight)
        {
            WeightedEdge<T> edge = new WeightedEdge<T>();

            if (home == null || neighbor == null)
            {
                return;
            }

            home.neighbors.Add(neighbor);
            neighbor.neighbors.Add(home);

            home.AddEdge(edge);
            neighbor.AddEdge(edge);
            edge.RegisterProperties(home, neighbor, weight);
            m_edges.Add(edge);
        }

        public void AddEdge(T data1, T data2)
        {
            WeightedEdge<T> edge = new WeightedEdge<T>();
            AddEdge(FindNode(data1), FindNode(data2));
            edge.RegisterProperties(FindNode(data1), FindNode(data2), 0.0f);
            m_edges.Add(edge);
        }

        public void AddEdge(T homeData, T neighborData, float weight)
        {
            Node<T> home = FindNode(homeData);
            Node<T> neighbor = FindNode(neighborData);
            WeightedEdge<T> edge = new WeightedEdge<T>();

            AddEdge(home, neighbor);
            home.AddEdge(edge);
            neighbor.AddEdge(edge);
            edge.RegisterProperties(home, neighbor, weight);
            edges.Add(edge);
        }

        public float GetWeight(T homeData, T neighborData)
        {
            if (homeData != null && neighborData != null)
            {
                Node<T> home = FindNode(homeData);
                Node<T> neighbor = FindNode(neighborData);

                foreach (WeightedEdge<T> edge in home.edges)
                {
                    if (edge.neighbor == neighbor)
                    {
                        return (edge.weight != 0.0f) ? edge.weight : 0.0f;
                    }
                }
            }
            else
            {
                Debug.Log("Home data " + homeData.ToString() + " or Neighbor data " + neighborData.ToString() + " is null.");
                return 0.0f;
            }

            Debug.Log("No weight registered between " + homeData.ToString() + " and " + neighborData.ToString());
            return 0.0f;
        }

        public float GetWeight(Node<T> home, Node<T> neighbor)
        {
            if (home != null && neighbor != null)
            {
                foreach (WeightedEdge<T> edge in home.edges)
                {
                    if (edge.neighbor == neighbor)
                    {
                        return (edge.weight != 0.0f) ? edge.weight : 0.0f;
                    }
                }
            }
            else
            {
                Debug.Log("Home data " + home.ToString() + " or Neighbor data " + neighbor.ToString() + " is null.");
                return 0.0f;
            }

            Debug.Log("No weight registered between " + home.ToString() + " and " + neighbor.ToString());
            return 0.0f;
        }

        public void ResetNodesVisited()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].visited = false;
            }
        }
    }
}