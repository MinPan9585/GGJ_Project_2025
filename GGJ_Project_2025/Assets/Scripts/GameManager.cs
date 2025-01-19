using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f; // 游戏时长（秒）
    private float remainingTime; // 剩余时间
    public TextMeshProUGUI timerText; // 用于显示时间的 UI
    public Image progressBar; // 用于显示进度条的 Image（前景）
    public GameObject winPanel; // 胜利面板
    public GameObject losePanel; // 失败面板
    public GameObject endGamePanel; // 结束游戏面板（包含狗胜利和人胜利的图片）
    public GameObject dogWinImage; // 狗胜利图片
    public GameObject humanWinImage; // 人胜利图片
    public GameObject gameWindow; // 游戏窗口（需要在结束时关闭）

    private bool isGameOver = false;

    void Start()
    {
        remainingTime = gameDuration;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        endGamePanel.SetActive(false); // 初始化时隐藏结束面板

        // 初始化进度条
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f; // 初始填充为满
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimer();
        }

        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.Delete))
        {
            RestartGame();
        }
    }

    void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            EndGame(false); // 时间结束，失败
        }

        // 更新 UI
        if (timerText != null)
        {
            timerText.text = ((int)remainingTime).ToString();
        }

        // 更新进度条填充
        if (progressBar != null)
        {
            progressBar.fillAmount = remainingTime / gameDuration; // 根据剩余时间更新填充比例
        }
    }

    public void EndGame(bool playerWins)
    {
        isGameOver = true;

        // 关闭游戏窗口
        if (gameWindow != null)
        {
            gameWindow.SetActive(false);
        }

        // 显示结束面板
        endGamePanel.SetActive(true);

        // 根据胜利者显示对应的图片
        if (playerWins)
        {
            dogWinImage.SetActive(false);
            humanWinImage.SetActive(true); // 显示人胜利图片
        }
        else
        {
            dogWinImage.SetActive(true); // 显示狗胜利图片
            humanWinImage.SetActive(false);
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
