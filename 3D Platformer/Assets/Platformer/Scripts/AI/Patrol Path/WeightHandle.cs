using System.Collections.Generic;
using UnityEngine;
using ViTiet.DataStructure.Graph;

namespace AI.PatrolPath
{
    [ExecuteInEditMode]
    public class WeightHandle : MonoBehaviour
    {
        public Node<GameObject> self;
        public List<Vector3> textPositions;
        public List<float> weights;

        private void Start()
        {
            Initiate();
        }

        private void Initiate()
        {
            self = GetComponentInParent<PatrolPathGraph>().PathGraph.FindNode(gameObject);

            if (self == null) return;

            WeightedEdge<GameObject> currentEdge;
            Vector3 textPositionToNeighbor = new Vector3();
            Vector3 textPositionFromNeighbor = new Vector3();
            Vector3 homePosition;
            Vector3 neighborPosition;
            int edgeCount;
            float x, y, z;

            edgeCount = self.edges.Count;

            x = transform.position.x;
            y = transform.position.y;
            z = transform.position.z;

            for (int i = 0; i < edgeCount; i++)
            {
                currentEdge = self.edges[i];

                if (self.data == currentEdge.home.data) /*!currentEdge.neighbor.visited*/
                {
                    neighborPosition = currentEdge.neighbor.data.transform.position;

                    textPositionToNeighbor.x = (x + neighborPosition.x) / 2;
                    textPositionToNeighbor.y = (y + neighborPosition.y) / 2;
                    textPositionToNeighbor.z = (z + neighborPosition.z) / 2;

                    textPositions.Add(textPositionToNeighbor);
                    weights.Add(currentEdge.weight);
                }
                else if (self.data == currentEdge.neighbor.data) /*!currentEdge.neighbor.visited*/
                {
                    homePosition = currentEdge.home.data.transform.position;

                    textPositionFromNeighbor.x = (x + homePosition.x) / 2;
                    textPositionFromNeighbor.y = (y + homePosition.y) / 2;
                    textPositionFromNeighbor.z = (z + homePosition.z) / 2;

                    textPositions.Add(textPositionFromNeighbor);
                    weights.Add(currentEdge.weight);
                }
            }
        }
    }
}