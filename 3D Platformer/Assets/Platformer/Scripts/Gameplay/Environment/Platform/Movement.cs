using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;
using CustomMathLibrary;

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
        [Range(0f, 1f)]
        [SerializeField] private float speed = 0f;
        [SerializeField] private bool moving = false;

        private float step = 0f;
        private bool isGoingToEndPoint = true;
        
        private void Update()
        {
            Move();
        }

        private void Move()
        {
            if (!moving || !start || !end) return;

            //step = CalculateStep(step);
            step = CustomMathf.CalculateLerpValueClamp01(step, type, isGoingToEndPoint);

            if (step == 0 || step == 1)
                isGoingToEndPoint = !isGoingToEndPoint;

            if (bezier)
                transform.position = Vector3.Lerp(Vector3.Lerp(start.position, bezier.position, step), Vector3.Lerp(bezier.position, end.position, step), step);
            else
                transform.position = Vector3.Lerp(start.position, end.position, step);
        }

        private float CalculateStep(float step)
        {
            step = isGoingToEndPoint ? step + Time.deltaTime * speed : step - Time.deltaTime * speed;

            switch (type)
            {
                case Type.Linear:
                    step = Linear.InOut(step);
                    break;
                case Type.Quadratic:
                    step = Quadratic.InOut(step);
                    break;
                case Type.Cubic:
                    step = Cubic.InOut(step);
                    break;
                case Type.Quartic:
                    step = Quartic.InOut(step);
                    break;
                case Type.Quintic:
                    step = Quintic.InOut(step);
                    break;
                case Type.Sinusoidal:
                    step = Sinusoidal.InOut(step);
                    break;
                case Type.Exponential:
                    step = Exponential.InOut(step);
                    break;
                case Type.Circular:
                    step = Circular.InOut(step);
                    break;
                case Type.Elastic:
                    step = Elastic.InOut(step);
                    break;
                case Type.Back:
                    step = Back.InOut(step);
                    break;
                case Type.Bounce:
                    step = Bounce.InOut(step);
                    break;
                default:
                    return -1f;
            }
            
            return step = (isGoingToEndPoint ? Mathf.Min(step, 1) : Mathf.Max(step, 0));
        }
    }
}