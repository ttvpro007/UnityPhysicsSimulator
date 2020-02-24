using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [SerializeField] private Camera[] portalCam = null;
    [SerializeField] private Material[] cameraMat = null;
    
    private void Start()
    {
        for (int i = 0; i < portalCam.Length && i < cameraMat.Length; i++)
        {
            SetUpTexture(portalCam[i], cameraMat[i]);
        }
    }

    private static void SetUpTexture(Camera camera, Material cameraMaterial)
    {
        if (camera.targetTexture)
        {
            camera.targetTexture.Release();
        }

        camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMaterial.mainTexture = camera.targetTexture;
    }
}
