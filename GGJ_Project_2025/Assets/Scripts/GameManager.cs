using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float gameDuration = 120f; // 游戏时长（秒）
    private float remainingTime; // 剩余时间

    public Image progressBar; // 用于显示进度条的 Image（前景）
    public GameObject rotatingImage; // 需要旋转的自定义 UIImage
    public GameObject winPanel; // 胜利面板
    public GameObject losePanel; // 失败面板
    public GameObject endGamePanel; // 结束游戏面板（包含狗胜利和人胜利的图片）
    public GameObject dogWinImage; // 狗胜利图片
    public GameObject humanWinImage; // 人胜利图片
    public GameObject gameWindow; // 游戏窗口（需要在结束时关闭）

    public Color colorStage1 = Color.green; // 第一阶段颜色
    public Color colorStage2 = Color.yellow; // 第二阶段颜色
    public Color colorStage3 = Color.red; // 第三阶段颜色

    private bool isGameOver = false;
    public float initialRotationSpeed = 50f; // 初始旋转速度（可修改）
    private float currentRotationSpeed; // 当前旋转速度

    void Start()
    {
        Time.timeScale = 1f;

        remainingTime = gameDuration;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        endGamePanel.SetActive(false); // 初始化时隐藏结束面板
        gameWindow.SetActive(true);

        // 初始化进度条
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f; // 初始填充为满
            progressBar.color = colorStage1; // 初始颜色为第一阶段颜色
        }

        // 初始化旋转速度
        currentRotationSpeed = initialRotationSpeed;
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimer();
            RotateImage(); // 更新 UIImage 的旋转
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

        // 更新进度条填充
        if (progressBar != null)
        {
            float progress = remainingTime / gameDuration; // 根据剩余时间计算填充比例
            progressBar.fillAmount = progress;

            // 根据进度条阶段设置颜色和旋转速度
            if (progress > 2f / 3f) // 第一阶段
            {
                progressBar.color = colorStage1;
                currentRotationSpeed = initialRotationSpeed; // 恢复为初始速度
            }
            else if (progress > 1f / 3f) // 第二阶段
            {
                progressBar.color = colorStage2;
                currentRotationSpeed = initialRotationSpeed * 2f; // 速度变为 2 倍
            }
            else // 第三阶段
            {
                progressBar.color = colorStage3;
                currentRotationSpeed = initialRotationSpeed * 3f; // 速度变为 3 倍
            }
        }
    }

    void RotateImage()
    {
        if (rotatingImage != null)
        {
            // 让 UIImage 在 Z 轴方向上匀速旋转
            rotatingImage.transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
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
