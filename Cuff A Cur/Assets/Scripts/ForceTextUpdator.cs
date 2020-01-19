using UnityEngine;
using UnityEngine.UI;

public class ForceTextUpdator : MonoBehaviour
{
    [SerializeField] Text forceText = null;

    private void Start()
    {
        ForceBarUpdator.OnStopped += UpdateText;
    }

    private void OnDisable()
    {
        ForceBarUpdator.OnStopped -= UpdateText;
    }

    private void UpdateText(StoppedEventArgs eventArgs)
    {
        forceText.text = string.Format("Force: {0:0.0}", eventArgs.ForceMagnitude);
    }
}
