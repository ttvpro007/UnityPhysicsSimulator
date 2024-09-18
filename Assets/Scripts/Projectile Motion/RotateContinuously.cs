using UnityEngine;

public class RotateContinuously : MonoBehaviour
{
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, speed);
    }
}
