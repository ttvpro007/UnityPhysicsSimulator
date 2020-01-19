using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puncher : MonoBehaviour
{
    [SerializeField] GameObject punchingObject = null;

    public void Punch(float forceMagnitude)
    {
        Vector3 forceDirection = punchingObject.transform.TransformDirection(-Vector3.forward);
        
        punchingObject.GetComponent<Rigidbody>().AddForce(forceDirection * forceMagnitude);
    }
}