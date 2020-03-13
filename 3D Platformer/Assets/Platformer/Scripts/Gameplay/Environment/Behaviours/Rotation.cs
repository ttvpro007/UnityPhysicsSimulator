using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;
using CustomMathLibrary;

namespace Environment.Behaviours
{
    public class Rotation : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private Vector3 rotateAxis = Vector3.up;

        [Header("Settings")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private bool easing = true;
        [SerializeField] private Type type = Type.Linear;
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 10f;

        private float speed = 0f;
        private float lerpValue = 0f;
        private float step = 0f;
        private bool isZeroToOne  = true;

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            if (!rotate) return;

            CalculateSpeed();
            UpdateStep();
            ToggleBoolean(ref isZeroToOne, step == 0 || step == 1);
            CalculateLerpValue();

            transform.Rotate(rotateAxis, speed);
        }

        private void CalculateSpeed()
        {
            if (easing)
            {
                speed = Mathf.Lerp(minSpeed, maxSpeed, lerpValue) * Time.deltaTime;
            }
            else
            {
                speed = baseSpeed * Time.deltaTime;
            }
        }

        private void CalculateLerpValue()
        {
            if (easing)
            {
                lerpValue = CustomMathf.CalculateLerpValueClamp01(step, type, isZeroToOne);
            }
            else
            {
                lerpValue = step;
            }
        }

        private void UpdateStep()
        {
            step = isZeroToOne ? step + Time.deltaTime : step - Time.deltaTime;
            step = CustomMathf.ClampMinMax(0f, 1f, step);
        }

        private static void ToggleBoolean(ref bool boolean, bool toggleCondition)
        {
            boolean = (toggleCondition) ? !boolean : boolean;
        }
    }
}