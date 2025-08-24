using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
public class SpringHUD : MonoBehaviour
{
    [Header("Targets")]
    public Spring spring;                 // your Spring component
    //public SpringVisualizer visualizer;   // optional; for toggles

    [Header("Texts")]
    public TMP_Text dispText;
    public TMP_Text forceText;
    public TMP_Text lenText;
    public TMP_Text velText;

    [Header("Sliders")]
    [LabelText("K Slider")] public Slider kSlider;   // springConstant
    [LabelText("C Slider")] public Slider cSlider;   // dampingModifier
    [LabelText("L0 Slider")] public Slider l0Slider;  // trueLength

    [Header("Toggles (Visualizer)")]
    public Toggle anchorLineT;
    public Toggle restPosT;
    public Toggle displacementT;
    public Toggle forceT;
    public Toggle velocityT;
    public Toggle castRayT;

    // cache for formatting
    static readonly string F1 = "F1";
    static readonly string F2 = "F2";
    static readonly string F3 = "F3";

    SpringSnapshot _snap;
    void OnEnable()
    {
        if (spring)
        {
            spring.OnStepped -= HandleStep;
            spring.OnStepped += HandleStep;
        }
    }

    void OnDisable()
    {
        if (spring)
        {
            spring.OnStepped -= HandleStep;
        }
    }
    
    void HandleStep(SpringSnapshot s)
    {
        _snap = s;

        if (!spring) return;

        // Read once
        float x = _snap.displacement;               // m (positive = compression)
        float F = _snap.forceScalar;                // N  (scalar along line)
        float L = _snap.currentLength;              // m
        float xDt = _snap.compressionVelocity;      // m/s
        Vector3 v = _snap.rbPointVelocity;

        // Quick health metric: damping ratio ζ ≈ c / (2√(k m))
        float m = spring.rb ? Mathf.Max(1e-6f, spring.rb.mass) : 1f;
        float zeta = (spring.dampingModifier) / (2f * Mathf.Sqrt(Mathf.Max(1e-6f, spring.springConstant) * m));

        // Update labels
        if (dispText) dispText.text = $"x: {x.ToString(F2)} m   xDt: {xDt.ToString(F2)} m/s\nzeta: {zeta.ToString(F2)}";
        if (forceText) forceText.text = $"F: {F.ToString(F1)} N";
        if (lenText) lenText.text = $"L: {L.ToString(F2)} m   L0: {spring.trueLength.ToString(F2)} m";
        if (velText) velText.text = $"v: ({v.x.ToString(F2)}, {v.y.ToString(F2)}, {v.z.ToString(F2)}) m/s";
    }


    // Update the calls to BindToggle to pass UnityAction<bool> instead of System.Action<bool>
    void Awake()
    {
        if (!spring) spring = FindAnyObjectByType<Spring>();

        // Initialize slider ranges & values sensibly
        if (kSlider)
        {
            kSlider.minValue = 0f; kSlider.maxValue = 10000f;
            kSlider.SetValueWithoutNotify(spring.springConstant);
            kSlider.onValueChanged.AddListener(v => spring.SetSpringConstant(v));
        }
        if (cSlider)
        {
            cSlider.minValue = 0f; cSlider.maxValue = 50f;
            cSlider.SetValueWithoutNotify(spring.dampingModifier);
            cSlider.onValueChanged.AddListener(v => spring.SetDampingModifier(v));
        }
        if (l0Slider)
        {
            l0Slider.minValue = 0f; l0Slider.maxValue = 20f;
            l0Slider.SetValueWithoutNotify(spring.trueLength);
            l0Slider.onValueChanged.AddListener(v => spring.SetTrueLength(v));
        }
    }
}

static class StringFmtExt
{
    public static string F2Safe(this float value)
    {
        // nicer small-number readout, avoids "-0.00"
        float v = Mathf.Abs(value) < 1e-4f ? 0f : value;
        return v.ToString("F2");
    }
}
