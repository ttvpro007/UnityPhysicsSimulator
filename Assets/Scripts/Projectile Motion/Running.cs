using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Running : MonoBehaviour
{
    [SerializeField] bool run;
    [SerializeField] float speed;
    [SerializeField] Transform mapCenter;
    [SerializeField] float radius;
    [SerializeField] float destinationTolerance;

    NavMeshAgent agent;
    Vector3 currentDestination;

    private void Start()
    {
        Initiate();
    }

    private void Initiate()
    {
        if (!mapCenter) { Debug.Log("Map Center is nor referenced."); return; }
        if (speed == 0) speed = 5f;
        if (radius == 0) radius = 10f;
        if (destinationTolerance == 0) destinationTolerance = 2f;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        SetDestination();
    }

    private void Update()
    {
        if (!mapCenter) return;

        if (!run) return;
        Run();
    }

    private void Run()
    {
        if (Vector3.Distance(transform.position, currentDestination) <= destinationTolerance)
        {
            SetDestination();
        }
    }

    private void SetDestination()
    {
        currentDestination = generateRandomPosition();
        agent.destination = currentDestination;
    }

    Vector3 generateRandomPosition()
    {
        float x, y, z;

        Vector3 randomPosition = Vector3.zero;

        do
        {
            x = Random.Range(-radius, radius);
            y = 0;
            z = Random.Range(-radius, radius);

            randomPosition = new Vector3(x, y, z);
        }
        while (Vector3.Distance(mapCenter.position, randomPosition) <= radius);

        return randomPosition;
    }
}
