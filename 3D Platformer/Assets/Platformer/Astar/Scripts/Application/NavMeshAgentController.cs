using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshAgentController : MonoBehaviour
{
    [SerializeField] private Transform destinationTransform = null;

    private CustomNavMeshAgent agent = null;

    private void Start()
    {
        agent = GetComponent<CustomNavMeshAgent>();
    }

    private void Update()
    {
        agent.MoveTo(destinationTransform.position);
    }
}