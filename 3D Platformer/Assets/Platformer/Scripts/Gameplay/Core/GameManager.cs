using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }

    private static bool isPlaying = false;
    public static bool IsPlaying { get { return isPlaying; } }

    [SerializeField] private Transform player = null;
    [SerializeField] private Transform goal = null;
    [SerializeField] private Text winText = null;
    [SerializeField] private ScoreTextUpdator scoreText = null;
    private bool isQuitting = false;

    private float score = 0f;

    private void Awake()
    {
        if (!instance) instance = this;
        else if (instance == this) Destroy(gameObject);
    }

    private void Start()
    {
        isPlaying = true;

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    private void Update()
    {
        if (WinCondition())
        {
            winText.gameObject.SetActive(true);
            Debug.Log(winText.text);
            if (!isQuitting) StartCoroutine(QuitGame(3));
        }

        scoreText.UpdateText("" + score);
    }

    public void AddToScore(float amount)
    {
        score += amount;
    }

    private IEnumerator QuitGame(float timer)
    {
        isQuitting = true;
        yield return new WaitForSeconds(timer);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool WinCondition()
    {
        return Vector3.Distance(player.position, goal.position) <= 1;
    }
}
