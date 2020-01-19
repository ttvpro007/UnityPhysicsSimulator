using UnityEngine;
using UnityEngine.UI;

public class ScoreTextUpdator : MonoBehaviour
{
    [SerializeField] Text ScoreText = null;
    private float score = 0;
    private static float totalScore = 0;
    private bool isStopped = true;

    private void Start()
    {
        ForceApplier.OnStarted += StartCounting;
        ForceApplier.OnStopped += StopCounting;
        InputRegister.OnReset += Reset;
    }

    private void OnDisable()
    {
        ForceApplier.OnStarted -= StartCounting;
        ForceApplier.OnStopped -= StopCounting;
        InputRegister.OnReset -= Reset;
    }

    private void Update()
    {
        score = ForceApplier.Instance.Velocity * Time.deltaTime;

        if (isStopped || score == 0) return;

        totalScore += score;
        ScoreText.text = string.Format("Score: {0:0.00}", totalScore);
    }

    private void StartCounting()
    {
        isStopped = false;
    }

    private void StopCounting()
    {
        isStopped = true;
    }

    public static void Reset()
    {
        totalScore = 0;
    }
}
