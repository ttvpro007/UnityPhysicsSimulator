using System;
using UnityEngine;

public class ForceBarUpdator : MonoBehaviour
{
    // event delegate
    public delegate void OnStoppedEvent(StoppedEventArgs eventArgs);
    public static event OnStoppedEvent OnStopped;

    // static variables
    private static ForceBarUpdator instance = null;
    public static ForceBarUpdator Instance { get { return instance; } }

    // changable private variables in inspector
    [SerializeField] private float secondToFilled = 0;
    [SerializeField] private RectTransform fillBar = null;

    // private variables
    private float maxForceMagnitude = 0;
    private float forceMagnitude = 0;
    private Vector3 currentScale = Vector3.zero;
    private UpdatorState state = UpdatorState.Up;

    private void Awake()
    {
        instance = this;   
    }

    private void Start()
    {
        Initiate();
        Reset();
        InputRegister.OnReset += Restart;
    }

    private void OnDisable()
    {
        InputRegister.OnReset -= Restart;
    }

    private void Update()
    {
        UpdateFillBar();
    }

    private void Initiate()
    {
        maxForceMagnitude = GameManager.Instance.MaxForce;
    }

    private void Reset()
    {
        state = UpdatorState.Up;
        currentScale = fillBar.localScale;
        currentScale.x = 0;
    }

    private void UpdateFillBar()
    {
        if (state == UpdatorState.Stop) return;

        state = StateValueLerping.GetLerpingState(state, currentScale.x);
        currentScale.x = Mathf.Min(StateValueLerping.StateLerping01(state, currentScale.x, secondToFilled), 1);
        fillBar.localScale = currentScale;
    }

    public void Stop()
    {
        if (state == UpdatorState.Stop)
            return;
        state = UpdatorState.Stop;

#if UNITY_EDITOR
        // in case value changed in GameManager
        maxForceMagnitude = Mathf.Max(1, GameManager.Instance.MaxForce);
#endif

        //force = ForceEvaluator.Evaluate(maxForce, currentScale.x, EvaluateMethod.Parabola);
        forceMagnitude = ForceEvaluator.Evaluate(maxForceMagnitude, currentScale.x, EvaluateMethod.Linear);
        OnStopped.Invoke(new StoppedEventArgs(state, currentScale.x, forceMagnitude));
    }

    public void Restart()
    {
        Reset();
    }
}

public class StoppedEventArgs
{
    public StoppedEventArgs(UpdatorState state, float lerpValue, float forceMagnitude)
    {
        LerpValue = lerpValue;
        ForceMagnitude = forceMagnitude;
    }
    
    public float LerpValue { get; }
    public float ForceMagnitude { get; }
}