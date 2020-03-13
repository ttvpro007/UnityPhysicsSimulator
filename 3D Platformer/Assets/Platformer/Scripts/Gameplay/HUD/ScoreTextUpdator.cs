using UnityEngine;
using UnityEngine.UI;

public class ScoreTextUpdator : MonoBehaviour
{
    [SerializeField] private Text scoreText = null;

    public void UpdateText(string score)
    {
        scoreText.text = "Score: " + score;
    }
}
