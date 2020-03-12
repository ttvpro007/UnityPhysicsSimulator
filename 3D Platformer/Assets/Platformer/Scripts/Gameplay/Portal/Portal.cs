using PhysicsSimulation.Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PairedPortal
{
    [ExecuteInEditMode]
    public class Portal : MonoBehaviour
    {
        [Header("Linked Portal Properties")]
        [SerializeField] private Portal linkedPortal = null;

        [Header("Camera Properties")]
        [SerializeField] private PortalCamera portalCamera = null;
        [SerializeField] private Material cameraMaterital = null;
        [SerializeField] private RaycastHitInfo hitInfo = null;

        [Header("Screen Properties")]
        [SerializeField] private MeshRenderer screen = null;
        [SerializeField] private Material screenMaterital = null;

        private BoxCollider screenBoxCollider = null;
        
        public Portal LinkedPortal { get { return linkedPortal; } }
        public PortalCamera PortalCamera { get { return portalCamera; } }
        public Material CameraMaterital { get { return cameraMaterital; } }
        public MeshRenderer Screen { get { return screen; } }
        public BoxCollider ScreenBoxCollider { get { return screenBoxCollider; } }
        public Material ScreenMaterital { get { return screenMaterital; } }
        public RaycastHitInfo HitInfo { get { return hitInfo; } }

        private void Start()
        {
            linkedPortal.transform.rotation = transform.rotation;
            screenBoxCollider = Screen.GetComponent<BoxCollider>();
        }
    }
}