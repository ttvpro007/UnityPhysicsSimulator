using Obvious.Soap;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSOAPHelper : MonoBehaviour
{
    [System.Serializable]
    private class SliderFields
    {
        [SerializeField] private string name;
        [SerializeField] private TMP_Text nameField;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text valueField;
        [MinValue(0f)]
        [SerializeField] private float fineTuneValue;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;

        public void SetupSlider(ScriptableVariableBase sliderVariable)
        {
            // Set name
            nameField.text = name;

            if (sliderVariable is FloatVariable floatVariable)
            {
                // Set slider min max
                SetSliderMinMax(floatVariable.Min, floatVariable.Max);

                slider.wholeNumbers = false;

                // Set slider on changed callbacks
                slider.onValueChanged.AddListener(value => floatVariable.Value = value);

                // Bind TMP
                var bind = valueField.GetComponent<BindTextMeshPro>();
                bind.Type = CustomVariableType.Float;
                bind.DecimalAmount = 1;

                Type type = typeof(BindTextMeshPro);
                FieldInfo fieldInfo = type.GetField("_floatVariable", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(bind, floatVariable);
                }
            }
            else if (sliderVariable is IntVariable intVariable)
            {
                // Set slider min max
                SetSliderMinMax(intVariable.Min, intVariable.Max);

                slider.wholeNumbers = true;

                // Set slider on changed callbacks
                slider.onValueChanged.AddListener(value => intVariable.Value = (int)value);

                // Bind TMP
                var bind = valueField.GetComponent<BindTextMeshPro>();
                bind.Type = CustomVariableType.Int;

                Type type = typeof(BindTextMeshPro);
                FieldInfo fieldInfo = type.GetField("_intVariable", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(bind, intVariable);
                }
            }

            // Set fine tune button callbacks
            minusButton.onClick.AddListener(() => slider.value -= fineTuneValue);
            plusButton.onClick.AddListener(() => slider.value += fineTuneValue);
        }

        private void SetSliderMinMax(float min, float max)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = min;
            valueField.text = min.ToString();
        }
    }

    [HideLabel]
    [SerializeField] private SliderFields sliderFields;

    [ValidateInput("ValidateSliderScriptableVariable", "Slider Variable must be of type FloatVariable or IntVariable", InfoMessageType.Error)]
    [SerializeField] private ScriptableVariableBase sliderVariable;

    private void Start()
    {
        BindVariable();
    }

    [Button]
    private void BindVariable()
    {
        if (!ValidateSliderScriptableVariable(sliderVariable))
        {
            Debug.LogError("Slider Variable must be of type FloatVariable or IntVariable");
            return;
        }

        sliderFields.SetupSlider(sliderVariable);
    }

    private bool ValidateSliderScriptableVariable(ScriptableVariableBase scriptableVariable)
    {
        return scriptableVariable is FloatVariable || scriptableVariable is IntVariable;
    }
}