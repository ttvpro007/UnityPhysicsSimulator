using UnityEngine;

namespace Obvious.Soap.Example
{
    public class PositionSaver : MonoBehaviour
    {
        [SerializeField] private Vector3Variable _vector3Variable = null;

        private void Start()
        {
            transform.position = _vector3Variable.Value;
        }

        private void Update()
        {
            _vector3Variable.Value = transform.position;
        }
    }
}