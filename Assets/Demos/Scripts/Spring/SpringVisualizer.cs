using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Spring))]
public class SpringVisualizer : MonoBehaviour
{
    [Header("Spring")]
    public Spring spring;

    [Header("Toggles")]
    public bool drawAnchorLine = true;
    public bool drawRestPosition = true;   // anchor + n * trueLength
    public bool drawDisplacement = true;   // along line of action
    public bool drawForce = true;   // lastScalar vector
    public bool drawVelocity = true;   // point velocity at attach
    public bool drawCastRay = true;   // ray reach used this step

    [Header("Scales")]
    public float forceVizScale = 0.002f; // convert N  -> meters
    public float velocityVizScale = 0.1f;   // convert m/s -> meters

    void OnEnable() => EnsureSpringRef();

#if UNITY_EDITOR
    void OnValidate() => EnsureSpringRef();

    void OnDrawGizmosSelected()
    {
        if (!spring) return;

        // 1) Gather context
        Vector3 attach = transform.position;
        bool anchorValid;
        Vector3 anchor = ResolveAnchor(out anchorValid);
        Vector3 n; float L;
        ComputeLineOfAction(attach, anchor, anchorValid, out n, out L);

        // 2) Draw pieces
        if (drawAnchorLine) DrawAnchorLine(attach, anchor, anchorValid);
        if (drawRestPosition) DrawRestPosition(anchor, n, anchorValid);
        if (drawDisplacement) DrawDisplacement(attach, n, anchorValid);
        if (drawForce) DrawForce(attach, n, anchorValid);
        if (drawVelocity) DrawVelocity(attach);
        if (drawCastRay) DrawCastRay(attach);

        DrawLabels(attach);
    }
#endif

    // ===================== Helpers =====================

    void EnsureSpringRef()
    {
        if (!spring) spring = GetComponent<Spring>();
    }

#if UNITY_EDITOR
    Vector3 ResolveAnchor(out bool anchorValid)
    {
        anchorValid = false;
        Vector3 anchor = spring.lastAnchorWorld;

        switch (spring.anchorMode)
        {
            case Spring.AnchorMode.WorldPoint:
                anchor = spring.worldAnchorPoint;
                anchorValid = true;
                break;

            case Spring.AnchorMode.Transform:
                if (spring.anchorTransform)
                {
                    anchor = spring.anchorTransform.TransformPoint(spring.anchorLocalOffset);
                    anchorValid = true;
                }
                break;

            case Spring.AnchorMode.StickyHit:
            case Spring.AnchorMode.None:
                // Valid this step if we actually had a usable length/hit.
                anchorValid = spring.currentLength > 1e-4f;
                // anchor already taken from lastAnchorWorld
                break;
        }

        return anchor;
    }

    void ComputeLineOfAction(in Vector3 attach, in Vector3 anchor, bool anchorValid, out Vector3 n, out float L)
    {
        n = Vector3.zero; L = 0f;
        if (!anchorValid) return;

        Vector3 dir = attach - anchor;
        L = dir.magnitude;
        if (L > 1e-6f) n = dir / L; // normalized from anchor -> attach
    }

    // --------------- Draw blocks ---------------

    void DrawAnchorLine(in Vector3 attach, in Vector3 anchor, bool anchorValid)
    {
        if (!anchorValid) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(attach, anchor);
        Gizmos.DrawSphere(anchor, 0.03f);
    }

    void DrawRestPosition(in Vector3 anchor, in Vector3 n, bool anchorValid)
    {
        if (!anchorValid || n == Vector3.zero) return;
        Vector3 restPos = anchor + n * spring.trueLength;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.9f);
        Gizmos.DrawWireSphere(restPos, 0.025f);
        Gizmos.DrawLine(anchor, restPos);
    }

    void DrawDisplacement(in Vector3 attach, in Vector3 n, bool anchorValid)
    {
        if (!anchorValid || n == Vector3.zero) return;

        // x = L0 - L (compress>0, stretch<0)
        float x = Mathf.Clamp(spring.displacement, -spring.trueLength, spring.trueLength);
        Gizmos.color = (spring.displacement > 0f) ? Color.yellow : Color.cyan;
        Gizmos.DrawLine(attach, attach + (-n * x)); // signed along line-of-action
    }

    void DrawForce(in Vector3 attach, in Vector3 n, bool anchorValid)
    {
        if (!anchorValid || n == Vector3.zero) return;

        Vector3 F = spring.lastScalar * n;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(attach, attach + F * forceVizScale);

        if (F != Vector3.zero)
        {
            Handles.color = Color.magenta;
            Handles.ArrowHandleCap(0, attach, Quaternion.LookRotation(F), F.magnitude * forceVizScale, EventType.Repaint);
        }
    }

    void DrawVelocity(in Vector3 attach)
    {
        if (!spring.rb) return;

        Vector3 v = spring.rb.GetPointVelocity(attach);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(attach, attach + v * velocityVizScale);

        if (v != Vector3.zero)
        {
            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, attach, Quaternion.LookRotation(v), v.magnitude * velocityVizScale, EventType.Repaint);
        }
    }

    void DrawCastRay(in Vector3 attach)
    {
        float reach = Mathf.Max(0f, spring.maxCast);
        Vector3 rayDir = -transform.up;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        Gizmos.DrawLine(attach, attach + rayDir * reach);
        Gizmos.DrawWireSphere(attach + rayDir * reach, 0.015f);
    }

    void DrawLabels(in Vector3 attach)
    {
        Handles.color = Color.white;
        Handles.Label(attach + Vector3.up * 0.05f,
            $"x={spring.displacement:F3}m\nF={spring.lastScalar:F1}N\nL={spring.currentLength:F3}m");
    }
#endif
}
