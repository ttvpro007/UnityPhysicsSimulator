using UnityEngine;

[RequireComponent(typeof(Spring))]
public class SpringVisualizer : MonoBehaviour
{
    [Header("Spring")]
    public Spring spring;

    void OnEnable()
    {
        if (!spring) spring = GetComponent<Spring>();
    }

    private void OnValidate()
    {
        if (!spring) spring = GetComponent<Spring>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(spring.transform.position, -spring.transform.up * Mathf.Min(spring.currentLength, spring.maxCast));

        Gizmos.color = spring.displacement >= 0 ? Color.yellow : Color.cyan;
        Gizmos.DrawLine(spring.transform.position, spring.transform.up * Mathf.Clamp(spring.displacement, -spring.trueLength, spring.trueLength));
    }
}
