using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    [SerializeField] GameObject punchingObject = null;
    [SerializeField] Color originalColor = Color.white;
    [SerializeField] Color changeToColor = Color.white;

    private void Start()
    {
        InputRegister.OnReset += Reset;
        Reset();
    }

    private void OnDisable()
    {
        InputRegister.OnReset -= Reset;
    }

    private void Reset()
    {
        GetComponent<Renderer>().material.color = originalColor;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.name == punchingObject.name)
        {
            GetComponent<Renderer>().material.color = changeToColor;
        }
    }
}
