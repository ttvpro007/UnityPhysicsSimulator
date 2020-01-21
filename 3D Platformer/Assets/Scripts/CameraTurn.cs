using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTurn : MonoBehaviour
{
    [SerializeField] float speed = 0;
    private float pitch, yaw;

    private void LateUpdate()
    {
        pitch -= Input.GetAxis("Mouse Y") * speed;
        yaw += Input.GetAxis("Mouse X") * speed;

        transform.eulerAngles = new Vector3(pitch, yaw, 0);
    }
}
