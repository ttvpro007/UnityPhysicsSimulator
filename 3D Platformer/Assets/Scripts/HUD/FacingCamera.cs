using UnityEngine;

namespace HUD
{
    public class FacingCamera : MonoBehaviour
    {
        [SerializeField] private Transform camera = null;

        private void Start()
        {
            if (!camera) camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(camera);
        }
    }
}