using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBController : MonoBehaviour
{
    Spring[] balancePoints;

    private void Start()
    {
        balancePoints = GetComponentsInChildren<Spring>();
    }
}
