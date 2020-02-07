using System.Collections.Generic;
using UnityEngine;
using ViTiet.DataStructure.Graph;

namespace AI.PatrolPath
{
    public class PatrolPathGraph : MonoBehaviour
    {
        [SerializeField] private bool random = true;
        [SerializeField] private List<GameObject> waypoints = new List<GameObject>();
        private Graph<GameObject> pathGraph = new Graph<GameObject>();
        public Graph<GameObject> PathGraph { get { return pathGraph; } }

        private void Awake()
        {
            MakePathGraph(random);
        }

        private void MakePathGraph(bool makeRandom)
        {
            if (makeRandom)
            {
                List<GameObject> temp = new List<GameObject>();

                for (int i = 0; i < transform.childCount; i++)
                {
                    temp.Add(transform.GetChild(i).gameObject);
                }

                for (int i = 0; i < temp.Count;)
                {
                    int j = Random.Range(0, temp.Count);

                    pathGraph.AddNode(temp[j]);

                    waypoints.Add(temp[j]);

                    temp.Remove(temp[j]);
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject temp = transform.GetChild(i).gameObject;
                    pathGraph.AddNode(temp);
                    waypoints.Add(temp);
                }
            }

            for (int i = 0; i < pathGraph.nodeCount; i++)
            {
                int j = GetNextIndex(transform.childCount, i);
                float distance = Vector3.Distance(waypoints[i].transform.position, waypoints[j].transform.position);
                pathGraph.AddEdge(waypoints[i], waypoints[j], distance);
            }
        }

        private void Update()
        {
            //for (int i = 0; i < pathGraph.nodeCount; i++)
            //{
            //    int j = GetNextIndex(pathGraph.nodeCount, i);
            //    Debug.DrawLine(GetWaypoint(i), GetWaypoint(j), Color.white);
            //}
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < pathGraph.nodeCount; i++)
            {
                int j = GetNextIndex(pathGraph.nodeCount, i);
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }
        }

        private int GetNextIndex(int maxSize, int currentIndex)
        {
            return (currentIndex == maxSize - 1) ? 0 : currentIndex + 1;
        }

        public int GetNextWaypointIndex(int currentIndex)
        {
            return (currentIndex == pathGraph.nodeCount - 1) ? 0 : currentIndex + 1;
        }

        public Vector3 GetWaypoint(int i)
        {
            return pathGraph.FindNode(waypoints[i]).data.transform.position;
        }
    }
}