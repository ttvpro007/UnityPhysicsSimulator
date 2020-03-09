using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarScene
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float smoothFactor = 15f;
        private Quaternion rotation;
        private float initialFOV = 0;

        private void Start()
        {
            initialFOV = transform.GetComponent<Camera>().fieldOfView;
        }

        private void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, GetCameraPosition(), smoothFactor * Time.fixedDeltaTime);

            // global rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, smoothFactor * Time.fixedDeltaTime);

            // look rotation
            rotation = Quaternion.FromToRotation(transform.forward, target.position - transform.position) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, smoothFactor * Time.fixedDeltaTime);
        }

        private Vector3 GetCameraPosition()
        {
            Vector3 position;
            position = target.position + target.right * offset.x + target.up * offset.y + target.forward * offset.z;
            return position;

        }
    }
}