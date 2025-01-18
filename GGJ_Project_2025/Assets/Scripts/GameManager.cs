using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f; // 游戏时长（秒）
    private float remainingTime; // 剩余时间
    public TextMeshProUGUI timerText; // 用于显示时间的 UI
    public GameObject winPanel; // 胜利面板
    public GameObject losePanel; // 失败面板

    private bool isGameOver = false;

    void Start()
    {
        remainingTime = gameDuration;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            EndGame(false); // 时间结束，狗胜利
        }

        // 更新 UI
        if (timerText != null)
        {
            timerText.text = $"Time: {remainingTime:F1}s";
        }
    }

    public void EndGame(bool playerWins)
    {
        isGameOver = true;

        // 显示胜利或失败面板
        if (playerWins)
        {
            winPanel.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
        }

        // 停止游戏
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

