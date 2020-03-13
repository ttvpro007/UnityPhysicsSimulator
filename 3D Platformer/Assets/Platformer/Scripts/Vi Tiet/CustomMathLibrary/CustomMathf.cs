using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;

namespace CustomMathLibrary
{
    public static class CustomMathf
    {
        public static float CalculateLerpValue(float lerpValue, Type easingType, bool isZeroToOne)
        {
            switch (easingType)
            {
                case Type.Linear:
                    lerpValue = Linear.InOut(lerpValue);
                    break;
                case Type.Quadratic:
                    lerpValue = Quadratic.InOut(lerpValue);
                    break;
                case Type.Cubic:
                    lerpValue = Cubic.InOut(lerpValue);
                    break;
                case Type.Quartic:
                    lerpValue = Quartic.InOut(lerpValue);
                    break;
                case Type.Quintic:
                    lerpValue = Quintic.InOut(lerpValue);
                    break;
                case Type.Sinusoidal:
                    lerpValue = Sinusoidal.InOut(lerpValue);
                    break;
                case Type.Exponential:
                    lerpValue = Exponential.InOut(lerpValue);
                    break;
                case Type.Circular:
                    lerpValue = Circular.InOut(lerpValue);
                    break;
                case Type.Elastic:
                    lerpValue = Elastic.InOut(lerpValue);
                    break;
                case Type.Back:
                    lerpValue = Back.InOut(lerpValue);
                    break;
                case Type.Bounce:
                    lerpValue = Bounce.InOut(lerpValue);
                    break;
                default:
                    return -1f;
            }

            return lerpValue;
        }

        public static float CalculateLerpValueClamp01(float lerpValue, Type easingType, bool isZeroToOne)
        {
            switch (easingType)
            {
                case Type.Linear:
                    lerpValue = Linear.InOut(lerpValue);
                    break;
                case Type.Quadratic:
                    lerpValue = Quadratic.InOut(lerpValue);
                    break;
                case Type.Cubic:
                    lerpValue = Cubic.InOut(lerpValue);
                    break;
                case Type.Quartic:
                    lerpValue = Quartic.InOut(lerpValue);
                    break;
                case Type.Quintic:
                    lerpValue = Quintic.InOut(lerpValue);
                    break;
                case Type.Sinusoidal:
                    lerpValue = Sinusoidal.InOut(lerpValue);
                    break;
                case Type.Exponential:
                    lerpValue = Exponential.InOut(lerpValue);
                    break;
                case Type.Circular:
                    lerpValue = Circular.InOut(lerpValue);
                    break;
                case Type.Elastic:
                    lerpValue = Elastic.InOut(lerpValue);
                    break;
                case Type.Back:
                    lerpValue = Back.InOut(lerpValue);
                    break;
                case Type.Bounce:
                    lerpValue = Bounce.InOut(lerpValue);
                    break;
                default:
                    return -1f;
            }

            lerpValue = ClampMinMax(0f, 1f, lerpValue);

            return lerpValue;
        }

        public static float ClampMinMax(float min, float max, float value)
        {
            if (value > max) value = max;
            else if (value < min) value = min;

            return value;
        }
    }
}