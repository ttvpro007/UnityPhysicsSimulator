using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingObjectManager : MonoBehaviour
{
    // static variables
    private static PunchingObjectManager instance = null;
    public static PunchingObjectManager Instance { get { return instance; } }

    [SerializeField] GameObject punchingObject = null;
    private Vector3 originalPosition = Vector3.zero;
    private Quaternion originalRotation = Quaternion.identity;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InputRegister.OnReset += Reset;
        originalPosition = punchingObject.transform.position;
        originalRotation = punchingObject.transform.rotation;
    }

    public void Reset()
    {
        punchingObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        punchingObject.GetComponent<Rigidbody>().freezeRotation = true;
        punchingObject.transform.position = originalPosition;
        punchingObject.transform.rotation = originalRotation;
        punchingObject.GetComponent<Rigidbody>().freezeRotation = false;
    }
}
