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
                step = CustomMathf.CalculateStepClamp01(step, type, isZeroToOne);
                isZeroToOne = (step == 0f || step == 1f) ? !isZeroToOne : isZeroToOne;
                speed = Mathf.Lerp(minSpeed, maxSpeed, step);
            }
            else
            {
                speed = baseSpeed;
            }
        }

        
    }
}