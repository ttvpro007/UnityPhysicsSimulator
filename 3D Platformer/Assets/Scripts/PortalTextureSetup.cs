using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [SerializeField] private Camera camera1 = null;
    [SerializeField] private Camera camera2 = null;
    [SerializeField] private Material cameraMat1 = null;
    [SerializeField] private Material cameraMat2 = null;
    
    private void Start()
    {
        SetUpTexture(camera1, cameraMat1);
        SetUpTexture(camera2, cameraMat2);
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
