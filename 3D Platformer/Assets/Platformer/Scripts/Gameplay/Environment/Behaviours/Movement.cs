using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;
using CustomMathLibrary;

namespace Environment.Behaviours
{
    public class Movement : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private Transform start = null;
        [SerializeField] private Transform bezier = null;
        [SerializeField] private Transform end = null;

        [Header("Settings")]
        [SerializeField] private bool move = true;
        [SerializeField] private bool easing = true;
        [SerializeField] private Type type = Type.Linear;
        [Range(0f, 1f)]
        [SerializeField] private float speed = 1f;
        
        private float lerpValue = 0f;
        private float step = 0f;
        private bool isZeroToOne = true;

        private void Update()
        {
            Move();
        }
        
        private void Move()
        {
            if (!move || !start || !end) return;

            UpdateStep();
            ToggleBoolean(ref isZeroToOne, step == 0 || step == 1);
            CalculateLerpValue();

            if (bezier)
                transform.position = Vector3.Lerp(Vector3.Lerp(start.position, bezier.position, lerpValue), Vector3.Lerp(bezier.position, end.position, lerpValue), lerpValue);
            else
                transform.position = Vector3.Lerp(start.position, end.position, lerpValue);
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
            step = isZeroToOne ? step + Time.deltaTime * speed : step - Time.deltaTime * speed;
            step = CustomMathf.ClampMinMax(0f, 1f, step);
        }

        private static void ToggleBoolean(ref bool boolean, bool toggleCondition)
        {
            boolean = toggleCondition ? !boolean : boolean;
        }
    }
}