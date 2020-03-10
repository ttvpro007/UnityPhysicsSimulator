using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interpolation.Easing;

namespace Environment.Platform
{
    public class Movement : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private Transform start = null;
        [SerializeField] private Transform bezier = null;
        [SerializeField] private Transform end = null;

        [Header("Settings")]
        [SerializeField] private Type type = Type.Linear;
        [SerializeField] private float speed = 0f;

        private float step = 0f;
        private bool isGoingToEndPoint = true;
        
        private void Update()
        {
            step = CalculateStep(step * speed);

            if (step == 0 || step == 1)
                isGoingToEndPoint = !isGoingToEndPoint;

            step = isGoingToEndPoint ? step + Time.deltaTime : step - Time.deltaTime;

            transform.position = Vector3.Lerp(Vector3.Lerp(start.position, bezier.position, step), Vector3.Lerp(bezier.position, end.position, step), step);
        }

        private float CalculateStep(float step)
        {
            switch (type)
            {
                case Type.Linear:
                    return Linear.InOut(step);
                case Type.Quadratic:
                    return Quadratic.InOut(step);
                case Type.Cubic:
                    return Cubic.InOut(step);
                case Type.Quartic:
                    return Quartic.InOut(step);
                case Type.Quintic:
                    return Quintic.InOut(step);
                case Type.Sinusoidal:
                    return Sinusoidal.InOut(step);
                case Type.Exponential:
                    return Exponential.InOut(step);
                case Type.Circular:
                    return Circular.InOut(step);
                case Type.Elastic:
                    return Elastic.InOut(step);
                case Type.Back:
                    return Back.InOut(step);
                case Type.Bounce:
                    return Bounce.InOut(step);
                default:
                    return -1f;
            }
        }
    }
}