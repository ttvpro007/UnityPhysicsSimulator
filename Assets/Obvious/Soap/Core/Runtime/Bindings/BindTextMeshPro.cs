﻿using System;
using System.Text;
using UnityEngine;
using TMPro;

namespace Obvious.Soap
{
    /// <summary>
    /// Binds a variable to a TextMeshPro component
    /// </summary>
    [AddComponentMenu("Soap/Bindings/BindTextMeshPro")]
    [RequireComponent(typeof(TMP_Text))]
    public class BindTextMeshPro : CacheComponent<TMP_Text>
    {
        public CustomVariableType Type = CustomVariableType.None;

        [SerializeField] private BoolVariable _boolVariable = null;
        [SerializeField] private IntVariable _intVariable = null;
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private StringVariable _stringVariable = null;

        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private Action<bool> _boolValueChangedHandler;
        private Action<int> _intValueChangedHandler;
        private Action<float> _floatValueChangedHandler;
        private Action<string> _stringValueChangedHandler;

        [Tooltip("Displays before the value")] public string Prefix = string.Empty;
        [Tooltip("Displays after the value")] public string Suffix = string.Empty;

        //int specific
        [Tooltip(
            "Useful too an offset, for example for Level counts. If your level index is  0, add 1, so it displays Level : 1")]
        public int Increment = 0;

        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public Vector2Int MinMaxInt = new Vector2Int(int.MinValue, int.MaxValue);

        //float specific
        [Min(1)] public int DecimalAmount = 2;

        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public bool IsClamped = false;

        public Vector2 MinMaxFloat = new Vector2(float.MinValue, float.MaxValue);


        protected override void Awake()
        {
            base.Awake();
            if (Type == CustomVariableType.None)
            {
                Debug.LogError("Select a type for this binding component", gameObject);
                return;
            }

            Refresh();
            Subscribe();
        }

        private void Refresh()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(Prefix);

            switch (Type)
            {
                case CustomVariableType.Bool:
                    _stringBuilder.Append(_boolVariable.Value ? "True" : "False");
                    break;
                case CustomVariableType.Int:
                    var clampedInt = IsClamped
                        ? Mathf.Clamp(_intVariable.Value, MinMaxInt.x, MinMaxInt.y)
                        : _intVariable.Value;
                    _stringBuilder.Append(clampedInt + Increment);
                    break;
                case CustomVariableType.Float:
                    double clampedFloat = IsClamped
                        ? Mathf.Clamp(_floatVariable.Value, MinMaxFloat.x, MinMaxFloat.y)
                        : _floatVariable.Value;
                    double rounded = System.Math.Round(clampedFloat, DecimalAmount);
                    _stringBuilder.Append(rounded);
                    break;
                case CustomVariableType.String:
                    _stringBuilder.Append(_stringVariable.Value);
                    break;
            }

            _stringBuilder.Append(Suffix);
            _component.text = _stringBuilder.ToString();
        }

        private void Subscribe()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    if (_boolVariable != null)
                    {
                        _boolValueChangedHandler = value => Refresh();
                        _boolVariable.OnValueChanged += _boolValueChangedHandler;
                    }
                    break;
                case CustomVariableType.Int:
                    if (_intVariable != null)
                    {
                        _intValueChangedHandler = value => Refresh();
                        _intVariable.OnValueChanged += _intValueChangedHandler;
                    }
                    break;
                case CustomVariableType.Float:
                    if (_floatVariable != null)
                    {
                        _floatValueChangedHandler = value => Refresh();
                        _floatVariable.OnValueChanged += _floatValueChangedHandler;
                    }
                    break;
                case CustomVariableType.String:
                    if (_stringVariable != null)
                    {
                        _stringValueChangedHandler = value => Refresh();
                        _stringVariable.OnValueChanged += _stringValueChangedHandler;
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    if (_boolVariable != null)
                        _boolVariable.OnValueChanged -= _boolValueChangedHandler;
                    break;
                case CustomVariableType.Int:
                    if (_intVariable != null)
                        _intVariable.OnValueChanged -= _intValueChangedHandler;
                    break;
                case CustomVariableType.Float:
                    if (_floatVariable != null)
                        _floatVariable.OnValueChanged -= _floatValueChangedHandler;
                    break;
                case CustomVariableType.String:
                    if (_stringVariable != null)
                        _stringVariable.OnValueChanged -= _stringValueChangedHandler;
                    break;
            }
        }
    }
}