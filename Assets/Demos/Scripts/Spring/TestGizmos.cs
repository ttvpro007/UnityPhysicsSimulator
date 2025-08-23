using UnityEngine;

public class TestGizmos : MonoBehaviour
{
    public RuntimeTransform runtimeTransform;

    private void Start()
    {
        if (runtimeTransform != null)
        {
            runtimeTransform.Attach(transform);
        }
    }
}