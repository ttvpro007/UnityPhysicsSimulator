using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Running : MonoBehaviour
{
    [SerializeField] bool run;
    [SerializeField] float speed = 5f;
    [SerializeField] float radius = 10f;
    [SerializeField] float destinationTolerance = 2f;
    [SerializeField] Transform mapCenter;
    [SerializeField] Transform directionIndicator;  // Object under the runner hierarchy that will show the running direction
    [SerializeField] Transform targetIndicator;

    private NavMeshAgent agent;
    private Vector3 currentDestination;

    public Vector3 CurrentVelocity { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (!mapCenter) { Debug.Log("Map Center is not referenced."); return; }

        agent.speed = speed;

        SetDestination();
    }

    private void Update()
    {
        if (!mapCenter) return;
        if (!run) return;

        // Update the current velocity of the agent
        CurrentVelocity = agent.velocity;

        // Check if too close to the target
        if (Vector3.Distance(transform.position, currentDestination) <= destinationTolerance)
        {
            SetDestination();
        }
    }

    private void SetDestination()
    {
        currentDestination = GenerateRandomPosition();
        agent.destination = currentDestination;
        SetTargetIndicatorPosition(agent.destination);
    }

    private Vector3 GenerateRandomPosition()
    {
        float x, y, z;
        Vector3 randomPosition;

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

    private void SetTargetIndicatorPosition(Vector3 position)
    {
        if (targetIndicator != null)
        {
            targetIndicator.transform.position = position;
        }
    }
}
