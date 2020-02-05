using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILook : MonoBehaviour
{
    [SerializeField] private Transform target = null;

    private void Update()
    {
        transform.LookAt(target);
    }
}