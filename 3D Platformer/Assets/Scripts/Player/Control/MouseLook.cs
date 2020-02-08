﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Control
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 100;
        [SerializeField] private Rigidbody bodyRB = null;

        private float xRotation = 0;
        private float yRotation = 0;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
            transform.localRotation = Quaternion.Euler(Vector3.right * xRotation);

            yRotation += mouseX;
            bodyRB.MoveRotation(Quaternion.Euler(Vector3.up * yRotation));
        }
    }
}