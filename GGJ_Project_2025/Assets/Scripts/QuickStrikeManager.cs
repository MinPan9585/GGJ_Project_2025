using UnityEngine;
using UnityEngine.UI;

public class QuickStrikeManager : MonoBehaviour
{
    public GameObject quickStrikePanel; // 快速打击时间的UI面板
    public Image ownerProgressBar; // 主人的进度条（Image组件）
    public Image dogProgressBar; // 狗的进度条（Image组件）

    public int ownerRequiredPresses = 10; // 主人需要按键的次数
    public int dogRequiredPresses = 8; // 狗需要按键的次数

    private int ownerCurrentPresses = 0; // 主人当前按键次数
    private int dogCurrentPresses = 0; // 狗当前按键次数

    private KeyCode ownerLastKey = KeyCode.None; // 主人上一次按下的键
    private KeyCode dogLastKey = KeyCode.None; // 狗上一次按下的键

    public bool isQuickStrikeActive = false; // 是否正在进行快速打击时间
    private GameManager gameManager; // 引用GameManager

    void Start()
    {
        quickStrikePanel.SetActive(false); // 默认隐藏快速打击面板
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isQuickStrikeActive)
        {
            HandleQuickStrikeInput();
        }
    }

    public void StartQuickStrike()
    {
        isQuickStrikeActive = true;
        quickStrikePanel.SetActive(true);

        // 初始化进度条和按键次数
        ownerProgressBar.fillAmount = 0f;
        dogProgressBar.fillAmount = 0f;
        ownerCurrentPresses = 0;
        dogCurrentPresses = 0;

        // 初始化上一次按键记录
        ownerLastKey = KeyCode.None;
        dogLastKey = KeyCode.None;
    }

    private void HandleQuickStrikeInput()
    {
        // 主人按左右方向键
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (ownerLastKey != KeyCode.LeftArrow) // 必须轮换按键
            {
                ownerCurrentPresses++;
                UpdateProgressBar(ownerProgressBar, ownerCurrentPresses, ownerRequiredPresses);
                ownerLastKey = KeyCode.LeftArrow; // 更新上一次按下的键
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (ownerLastKey != KeyCode.RightArrow) // 必须轮换按键
            {
                ownerCurrentPresses++;
                UpdateProgressBar(ownerProgressBar, ownerCurrentPresses, ownerRequiredPresses);
                ownerLastKey = KeyCode.RightArrow; // 更新上一次按下的键
            }
        }

        // 狗按A或D键
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (dogLastKey != KeyCode.A) // 必须轮换按键
            {
                dogCurrentPresses++;
                UpdateProgressBar(dogProgressBar, dogCurrentPresses, dogRequiredPresses);
                dogLastKey = KeyCode.A; // 更新上一次按下的键
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (dogLastKey != KeyCode.D) // 必须轮换按键
            {
                dogCurrentPresses++;
                UpdateProgressBar(dogProgressBar, dogCurrentPresses, dogRequiredPresses);
                dogLastKey = KeyCode.D; // 更新上一次按下的键
            }
        }

        // 检查胜负
        if (ownerCurrentPresses >= ownerRequiredPresses)
        {
            EndQuickStrike(true); // 主人胜利
        }
        else if (dogCurrentPresses >= dogRequiredPresses)
        {
            EndQuickStrike(false); // 狗胜利
        }
    }

    private void UpdateProgressBar(Image progressBar, int currentPresses, int requiredPresses)
    {
        // 根据当前按键次数更新进度条
        progressBar.fillAmount = (float)currentPresses / requiredPresses;
    }

    private void EndQuickStrike(bool ownerWins)
    {
        isQuickStrikeActive = false;
        quickStrikePanel.SetActive(false);

        if (ownerWins)
        {
            Debug.Log("主人抓住了狗！");
            gameManager.EndGame(true); // 主人胜利，结束游戏
        }
        else
        {
            Debug.Log("狗成功逃脱！");
            ResetPositions(); // 重置主人和狗的位置
        }
    }

    private void ResetPositions()
    {
        // 重置主人和狗的位置
        GameObject owner = GameObject.FindGameObjectWithTag("Player");
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

        owner.transform.position = new Vector3(5, .5f, 0); // 主人初始位置
        dog.transform.position = new Vector3(-5, .5f, 0); // 狗初始位置
    }
}



