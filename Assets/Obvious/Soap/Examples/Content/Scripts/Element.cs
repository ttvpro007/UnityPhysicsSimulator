using TMPro;
using UnityEngine;

namespace Obvious.Soap.Example
{
    [SelectionBase]
    public class Element : MonoBehaviour
    {
        [SerializeField] private ScriptableEnumElement _elementType = null;
        private ScriptableEnumElement ElementType => _elementType;

        private void Start()
        {
            GetComponent<Renderer>().material.color = _elementType.Color;
            GetComponentInChildren<TextMeshPro>().text = _elementType.Name;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent<Element>(out var element))
            {
                if (_elementType.Defeats.Contains(element.ElementType))
                    Destroy(other.gameObject);
            }
        }
    }
}