using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;

namespace CustomMathLibrary
{
    public static class CustomMathf
    {
        public static float CalculateStepClamp01(float step, Type easingType, bool isZeroToOne)
        {
            switch (easingType)
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

            step = isZeroToOne ? step + Time.deltaTime : step - Time.deltaTime;
            step = isZeroToOne ? Mathf.Min(step, 1f) : Mathf.Max(step, 0f);

            return step;
        }
    }
}