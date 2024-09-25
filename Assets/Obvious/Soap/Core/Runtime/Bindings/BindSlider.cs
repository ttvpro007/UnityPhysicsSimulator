using UnityEngine;
using UnityEngine.UI;

namespace Obvious.Soap
{
    /// <summary>
    /// Binds a float variable to a slider
    /// </summary>
    [AddComponentMenu("Soap/Bindings/BindSlider")]
    [RequireComponent(typeof(Slider))]
    public class BindSlider : CacheComponent<Slider>
    {
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private FloatVariable _maxValue = null;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(_floatVariable);
            if (_maxValue != null)
            {
                OnMaxValueChanged(_maxValue);
                _maxValue.OnValueChanged += OnMaxValueChanged;
            }

            _component.onValueChanged.AddListener(SetBoundVariable);
            _floatVariable.OnValueChanged += OnValueChanged;
        }
        
        private void OnDestroy()
        {
            _component.onValueChanged.RemoveListener(SetBoundVariable);
            _floatVariable.OnValueChanged -= OnValueChanged;
            if (_maxValue != null)
                _maxValue.OnValueChanged -= OnMaxValueChanged;
        }

        private void OnValueChanged(float value)
        {
            _component.value = value;
        }

        private void OnMaxValueChanged(float value)
        {
            _component.maxValue = value;
        }

        private void SetBoundVariable(float value)
        {
            _floatVariable.Value = value;
        }
    }
}