using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderValueTMPUGUIBinder : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    public int floatDigits = 2;

    private void OnEnable()
    {
        GetComponentRefs();
        UnregisterEvents();
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    [OnValidateCall]
    private void GetComponentRefs()
    {
        print("On Validate Call");
        slider ??= transform.GetComponentInSiblings<Slider>();
        text ??= GetComponent<TextMeshProUGUI>();
    }

    private void RegisterEvents()
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(HandleSliderValueChanged);
        }
    }

    private void UnregisterEvents()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(HandleSliderValueChanged);
        }
    }

    private void HandleSliderValueChanged(float value)
    {
        if (text != null)
        {
            if (floatDigits >= 0)
            {
                text.text = value.ToString("F" + floatDigits);
            }
            else
            {
                text.text = value.ToString();
            }
        }
    }

//#if UNITY_EDITOR
//    private void OnValidate()
//    {
//        GetComponentRefs();
//    }
//#endif
}

public static class TransformExtensions
{
    public static T GetComponentInSiblings<T>(this Transform component) where T : Component
    {
        if (!component || !component.parent) return null;
        return component.parent.GetComponentInChildren<T>();
    }
}
