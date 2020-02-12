using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    [SerializeField] private Transform camera = null;
    private RectTransform rTransform = null;

    private void Start()
    {
        if (!camera) camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(camera);
    }
}
