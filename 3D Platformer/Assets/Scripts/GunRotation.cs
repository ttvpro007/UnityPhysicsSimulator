using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRotation : MonoBehaviour
{
    [SerializeField] Transform camRotation = null;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = camRotation.rotation;
    }
}
