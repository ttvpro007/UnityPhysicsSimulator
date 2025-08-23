using UnityEngine;
using UnityEngine.EventSystems;

public enum Axis { None, X, Y, Z }

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class RuntimeTransformAxis : Interactible, IInteractible
{
    [Header("Axis")]
    public Axis axis = Axis.X;
}
