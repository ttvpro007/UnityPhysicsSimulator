using UnityEngine;

public class InputRegister : MonoBehaviour
{
    // event delegate
    public delegate void OnResetEvent();
    public static event OnResetEvent OnReset;

    [SerializeField] private KeyCode PunchButton = new KeyCode();
    [SerializeField] private KeyCode RestartButton = new KeyCode();
    private float forceMagnitude = 0;

    private void Start()
    {
        ForceBarUpdator.OnStopped += RegisterForceMagnitude;
        OnReset += Reset;
    }

    void Update()
    {
        if (Input.GetKeyDown(PunchButton))
        {
            ForceBarUpdator.Instance.Stop();

            if (forceMagnitude <= 0) return;

            ForceApplier.GetForceFromInputRegister(forceMagnitude);
            Reset();
        }
        else if (Input.GetKeyDown(RestartButton))
        {
            OnReset.Invoke();
            //ForceBarUpdator.Instance.Restart();
            //PunchingObjectManager.Instance.Reset();
            //ForceApplier.Reset();
            //ScoreTextUpdator.Reset();
            //Reset();
        }
    }

    private void RegisterForceMagnitude(StoppedEventArgs eventArgs)
    {
        forceMagnitude = eventArgs.ForceMagnitude;
    }

    private void Reset()
    {
        forceMagnitude = 0;
    }
}
