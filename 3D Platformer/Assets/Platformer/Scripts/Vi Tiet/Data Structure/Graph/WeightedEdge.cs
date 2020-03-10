namespace ViTiet.DataStructure.Graph
{
    public class WeightedEdge<T>
    {
        Node<T> m_home;
        Node<T> m_neighbor;
        float m_weight;

        public float weight { get { return m_weight; } }
        public Node<T> home { get { return m_home; } }
        public Node<T> neighbor { get { return m_neighbor; } }

        public void RegisterProperties(Node<T> home, Node<T> neighbor, float weight)
        {
            m_home = home;
            m_neighbor = neighbor;
            m_weight = weight;
        }

        public override string ToString()
        {
            return string.Format("Home: {0} - Distance: {1} meters - Neighbor: {2}", m_home.ToString(), weight.ToString("0.000"), m_neighbor.ToString());
        }
    }
}