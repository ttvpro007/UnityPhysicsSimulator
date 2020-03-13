using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomMathLibrary.Interpolation.Easing;
using CustomMathLibrary;

namespace Gameplay.PickUp
{
    public class Rotation : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Type type = Type.Linear;
        [SerializeField] private bool rotate = true;
        [SerializeField] private bool easing = true;
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private Vector3 axis = Vector3.up;

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
            transform.Rotate(axis, speed * Time.deltaTime);
        }

        private void CalculateSpeed()
        {
            if (easing)
            {
                UpdateStep();
                ToggleBoolean(ref isZeroToOne, step == 0 || step == 1);
                lerpValue = CalculateLerpValue();
                speed = Mathf.Lerp(minSpeed, maxSpeed, lerpValue);
            }
            else
            {
                speed = baseSpeed;
            }
        }

        private float CalculateLerpValue()
        {
            return CustomMathf.CalculateLerpValueClamp01(step, type, isZeroToOne);
        }

        private void ToggleBoolean(ref bool boolean, bool toggleCondition)
        {
            boolean = (toggleCondition) ? !boolean : boolean;
        }

        private void UpdateStep()
        {
            step = isZeroToOne ? step + Time.deltaTime : step - Time.deltaTime;
            //step = isZeroToOne ? Mathf.Min(step, 1f) : Mathf.Max(step, 0f);
            step = CustomMathf.ClampMinMax(0f, 1f, step);
        }

    }
}