using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class SpringHUD : MonoBehaviour
{
    [Header("Targets")]
    public Spring spring;                 // your Spring component
    public SpringVisualizer visualizer;   // optional; for toggles

    [Header("Texts")]
    public TMP_Text dispText;
    public TMP_Text forceText;
    public TMP_Text lenText;
    public TMP_Text velText;

    [Header("Sliders")]
    public Slider kSlider;   // springConstant
    public Slider cSlider;   // dampingModifier
    public Slider l0Slider;  // trueLength

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
    void OnEnable() { if (spring) spring.OnStepped += HandleStep; }
    void OnDisable() { if (spring) spring.OnStepped -= HandleStep; }
    void HandleStep(SpringSnapshot s)
    {
        _snap = s;

        if (!spring) return;

        // Read once
        float x = spring.displacement;               // m (positive = compression)
        float F = spring.lastScalar;                 // N  (scalar along line)
        float L = spring.currentLength;              // m
        float xDt = spring.compressionVelocity;        // m/s
        Vector3 v = spring.rb ? spring.rb.GetPointVelocity(spring.transform.position) : Vector3.zero;

        // Quick health metric: damping ratio ζ ≈ c / (2√(k m))
        float m = spring.rb ? Mathf.Max(1e-6f, spring.rb.mass) : 1f;
        float zeta = (spring.dampingModifier) / (2f * Mathf.Sqrt(Mathf.Max(1e-6f, spring.springConstant) * m));

        // Update labels
        if (dispText) dispText.text = $"x: {x.ToString(F3)} m   ẋ: {xDt.ToString(F3)} m/s   ζ: {zeta.ToString(F2)}";
        if (forceText) forceText.text = $"F: {F.ToString(F1)} N";
        if (lenText) lenText.text = $"L: {L.ToString(F3)} m   L₀: {spring.trueLength.ToString(F3)} m";
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
            kSlider.onValueChanged.AddListener(v => spring.springConstant = v);
        }
        if (cSlider)
        {
            cSlider.minValue = 0f; cSlider.maxValue = 100f;
            cSlider.SetValueWithoutNotify(spring.dampingModifier);
            cSlider.onValueChanged.AddListener(v => spring.dampingModifier = v);
        }
        if (l0Slider)
        {
            l0Slider.minValue = 0f; l0Slider.maxValue = 5f;
            l0Slider.SetValueWithoutNotify(spring.trueLength);
            l0Slider.onValueChanged.AddListener(v => spring.trueLength = v);
        }

        // Visualizer toggles
        if (visualizer)
        {
            BindToggle(anchorLineT, () => visualizer.drawAnchorLine, v => visualizer.drawAnchorLine = v);
            BindToggle(restPosT, () => visualizer.drawRestPosition, v => visualizer.drawRestPosition = v);
            BindToggle(displacementT, () => visualizer.drawDisplacement, v => visualizer.drawDisplacement = v);
            BindToggle(forceT, () => visualizer.drawForce, v => visualizer.drawForce = v);
            BindToggle(velocityT, () => visualizer.drawVelocity, v => visualizer.drawVelocity = v);
            BindToggle(castRayT, () => visualizer.drawCastRay, v => visualizer.drawCastRay = v);
        }
    }

    // Update the BindToggle method to use UnityAction<bool> instead of System.Action<bool>
    void BindToggle(Toggle t, System.Func<bool> get, UnityEngine.Events.UnityAction<bool> set)
    {
        if (!t) return;
        t.SetIsOnWithoutNotify(get());
        t.onValueChanged.AddListener(set);
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
