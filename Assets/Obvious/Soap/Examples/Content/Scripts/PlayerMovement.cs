﻿
using UnityEngine;

namespace Obvious.Soap.Example 
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Vector2Variable _inputs = null;
        [SerializeField] private FloatReference _speed = null;
        void Update()
        {
            var direction = new Vector3(_inputs.Value.x, 0, _inputs.Value.y);
            transform.position += direction * _speed * Time.deltaTime;
        }
    }
}