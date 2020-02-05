using System.Collections.Generic;
using UnityEngine;

public class PatrolPathGraph : MonoBehaviour
{
    [SerializeField] List<GameObject> waypoints = new List<GameObject>();
    public Graph<GameObject> graph = new Graph<GameObject>();

    private void Awake()
    {
        List<GameObject> temp = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            temp.Add(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < temp.Count;)
        {
            int j = Random.Range(0, temp.Count);

            graph.AddNode(temp[j]);

            waypoints.Add(temp[j]);

            temp.Remove(temp[j]);
        }

        for (int i = 0; i < graph.nodeCount; i++)
        {
            int j = GetNextIndex(transform.childCount, i);
            float distance = Vector3.Distance(waypoints[i].transform.position, waypoints[j].transform.position);
            graph.AddEdge(waypoints[i], waypoints[j], distance);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < graph.nodeCount; i++)
        {
            int j = GetNextIndex(graph.nodeCount, i);
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
        }
    }

    private int GetNextIndex(int maxSize, int currentIndex)
    {
        return (currentIndex == maxSize - 1) ? 0 : currentIndex + 1;
    }

    public int GetNextWaypointIndex(int currentIndex)
    {
        return (currentIndex == graph.nodeCount - 1) ? 0 : currentIndex + 1;
    }

    public Vector3 GetWaypoint(int i)
    {
        return graph.FindNode(waypoints[i]).data.transform.position;
    }
}