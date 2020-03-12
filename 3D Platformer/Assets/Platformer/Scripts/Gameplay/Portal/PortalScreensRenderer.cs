using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PairedPortal
{
    public class PortalScreensRenderer : MonoBehaviour
    {
        [SerializeField] private Portal[] portals = null;

        private void OnPreCull()
        {
            for (int i = 0; i < portals.Length; i++)
            {
                SetUpTexture(portals[i].PortalCamera.CameraComponent, portals[i].CameraMaterital);
                portals[i].PortalCamera.Render();
            }
        }

        private void SetUpTexture(Camera camera, Material cameraMaterial)
        {
            if (camera.targetTexture)
            {
                camera.targetTexture.Release();
            }
            
            camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
            cameraMaterial.mainTexture = camera.targetTexture;
        }
    }
}