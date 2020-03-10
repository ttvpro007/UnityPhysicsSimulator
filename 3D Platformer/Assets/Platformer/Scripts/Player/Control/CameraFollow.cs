using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Control
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target = null;

        // Start is called before the first frame update
        private void Start()
        {
            SetNewTransform();
        }

        private void LateUpdate()
        {
            SetNewTransform();
        }

        private void SetNewTransform()
        {
            if (!target) return;
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}