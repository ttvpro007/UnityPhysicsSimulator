using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using Obvious.Soap.Attributes;
#endif
namespace Obvious.Soap
{
    [CreateAssetMenu(fileName = "scriptable_variable_float.asset", menuName = "Soap/ScriptableVariables/float")]
    public class FloatVariable : ScriptableVariable<float>
    {
#if ODIN_INSPECTOR
        [PropertyOrder(5)]
#endif
        [Tooltip("Clamps the value of this variable to a minimum and maximum.")] 
        [SerializeField] private bool _isClamped = false;
        public bool IsClamped => _isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] 
        [SerializeField][ShowIf("_isClamped",true)]
#if ODIN_INSPECTOR
        [PropertyOrder(6)][Indent]
#endif
        private Vector2 _minMax = new Vector2(0, float.MaxValue);
        
        public Vector2 MinMax { get => _minMax; set => _minMax = value;}
        public float Min { get => _minMax.x; set => _minMax.x = value;}
        public float Max { get => _minMax.y; set => _minMax.y = value;}
        
        /// <summary>
        /// Returns the percentage of the value between the minimum and maximum.
        /// </summary>
        public float Ratio => Mathf.InverseLerp(_minMax.x, _minMax.y, _value);

        public override void Save()
        {
            PlayerPrefs.SetFloat(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = PlayerPrefs.GetFloat(Guid, DefaultValue);
            base.Load();
        }

        public void Add(float value)
        {
            Value += value;
        }

        public override float Value
        {
            set
            {
                var clampedValue = _isClamped ? Mathf.Clamp(value, _minMax.x, _minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_isClamped)
            {
                var clampedValue = Mathf.Clamp(Value, _minMax.x, _minMax.y);
                if (Value < clampedValue || Value > clampedValue)
                    Value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}