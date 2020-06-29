using UnityEngine;

namespace Player.Control
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField] private float mouseSensitivity = 100;
        [SerializeField] private Rigidbody bodyRB = null;

        private float xRotation = 0;
        private float yRotation = 0;

        public float YRotation { get { return yRotation; } }
        public float XRotation { get { return xRotation; } }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
            transform.localRotation = Quaternion.Euler(Vector3.right * xRotation);

            yRotation += mouseX;
            bodyRB.MoveRotation(Quaternion.Euler(Vector3.up * yRotation));
        }

        public void RotateY(float angle)
        {
            yRotation += angle;
            bodyRB.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        }
    }
}