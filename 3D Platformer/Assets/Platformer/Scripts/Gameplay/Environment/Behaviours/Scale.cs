using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;
using CustomMathLibrary;

namespace Environment.Behaviours
{
    public class Scale : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private bool scaleX = true;
        [SerializeField] private bool scaleY = true;
        [SerializeField] private bool scaleZ = true;

        [Header("Settings")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private bool easing = true;
        [SerializeField] private Type type = Type.Linear;
        [Range(0f, 1f)]
        [SerializeField] private float scaleSpeed = 1f;
        [SerializeField] private float minScale = 1f;
        [SerializeField] private float maxScale = 10f;
        
        private Vector3 minScale3D = Vector3.zero;
        private Vector3 maxScale3D = Vector3.zero;
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

            UpdateStep();
            ToggleBoolean(ref isZeroToOne, step == 0 || step == 1);
            CalculateLerpValue();

            CalculateMinMaxScale();

            transform.localScale = Vector3.Lerp(minScale3D, maxScale3D, lerpValue);
        }

        private void CalculateMinMaxScale()
        {
            minScale3D.x = scaleX ? minScale : 1;
            minScale3D.y = scaleY ? minScale : 1;
            minScale3D.z = scaleZ ? minScale : 1;

            maxScale3D.x = scaleX ? maxScale : 1;
            maxScale3D.y = scaleY ? maxScale : 1;
            maxScale3D.z = scaleZ ? maxScale : 1;
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
            step = isZeroToOne ? step + Time.deltaTime * scaleSpeed : step - Time.deltaTime * scaleSpeed;
            step = CustomMathf.ClampMinMax(0f, 1f, step);
        }

        private static void ToggleBoolean(ref bool boolean, bool toggleCondition)
        {
            boolean = (toggleCondition) ? !boolean : boolean;
        }
    }
}