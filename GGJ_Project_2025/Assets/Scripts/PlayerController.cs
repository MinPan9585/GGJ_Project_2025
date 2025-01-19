using UnityEngine;
using UnityEngine.UI; // 用于操作 UI
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 主人移动速度
    public float slipTime = 1f; // 滑倒时间
    public GameObject SlipBar; // 滑倒条
    public GameObject SpeedSkillBarContainer; // 控制技能条的显示/隐藏
    public GameObject SpeedSkillIconContainer; // 控制技能图标的显示/隐藏
    public Image SpeedCooldownBar; // 加速技能冷却条（Image组件，用于填充动画）
    public GameObject SpeedReadyPrompt; // 提示玩家按P激活技能
    public Image SpeedSkillBar; // 加速技能条（Image组件，用于填充动画）
    public RectTransform SkillIcon; // 技能条上的图标（RectTransform，用于动态移动）
    public Vector2 SkillIconLeftPosition; // 图标最左侧的位置
    public Vector2 SkillIconRightPosition; // 图标最右侧的位置
    public float speedMultiplier = 2f; // 加速倍数
    public float speedDuration = 2f; // 加速持续时间
    public float cooldownTime = 5f; // 冷却时间

    private Rigidbody rb; // 主人的刚体
    private bool canMove = true; // 是否可以移动（用于滑倒时禁用移动）
    private bool isSpeedSkillUnlocked = false; // 是否解锁了加速技能
    private bool isSpeedSkillReady = false; // 是否可以激活加速技能
    private bool isSpeedActive = false; // 是否正在使用加速技能
    private QuickStrikeManager strikeSc;
    public Transform spriteTransform; // 子对象的Transform，用于翻转动画

    void Start()
    {
        strikeSc = FindObjectOfType<QuickStrikeManager>();
        rb = GetComponent<Rigidbody>();

        // 如果没有手动设置 spriteTransform，可以尝试自动获取
        if (spriteTransform == null)
        {
            spriteTransform = transform.GetChild(0); // 假设子对象是第一个子物体
        }

        SlipBar.SetActive(false);
        SpeedSkillIconContainer.SetActive(false); // 初始隐藏技能图标
        SpeedSkillBarContainer.SetActive(false); // 初始隐藏技能条
        SpeedCooldownBar.fillAmount = 0; // 初始化冷却条为0
        SpeedReadyPrompt.SetActive(false);
        SpeedSkillBar.fillAmount = 0; // 初始化技能条为0
        UpdateSkillIconPosition(0); // 初始化图标位置
    }

    void Update()
    {
        if (canMove && !strikeSc.isQuickStrikeActive)
        {
            Move();
        }

        // 检测玩家是否按下P键激活加速技能
        if (isSpeedSkillUnlocked && isSpeedSkillReady && Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(ActivateSpeedSkill());
        }
    }

    void Move()
    {
        // 获取WASD输入
        float moveX = Input.GetAxis("HorizontalArrow"); // left right up down
        float moveZ = Input.GetAxis("VerticalArrow");

        // 设置刚体速度
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        rb.velocity = moveDirection * moveSpeed;

        // 翻转动画
        if (moveX != 0) // 只有在水平移动时才翻转
        {
            Vector3 localScale = spriteTransform.localScale;
            localScale.x = moveX > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            spriteTransform.localScale = localScale;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // 检测与拖把的碰撞
        if (collider.gameObject.CompareTag("Mop"))
        {
            UnlockSpeedSkill();
            Destroy(collider.gameObject); // 拾取拖把后销毁
        }
        // 检测与肥皂的碰撞
        if (collider.gameObject.CompareTag("Soap"))
        {
            StartCoroutine(Slip());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dog"))
        {
            Debug.Log("Catch the dog!");
            strikeSc.StartQuickStrike();
        }
    }

    private IEnumerator Slip()
    {
        canMove = false; // 禁用移动
        SlipBar.SetActive(true);
        rb.velocity = Vector3.zero; // 停止移动
        yield return new WaitForSeconds(slipTime); // 滑倒持续1秒
        canMove = true; // 恢复移动
        SlipBar.SetActive(false);
    }

    private void UnlockSpeedSkill()
    {
        isSpeedSkillUnlocked = true;
        SpeedSkillIconContainer.SetActive(true); // 显示技能图标
        StartCoroutine(SpeedCooldown());
    }

    private IEnumerator SpeedCooldown()
    {
        SpeedCooldownBar.fillAmount = 0; // 冷却条从0开始
        isSpeedSkillReady = false;

        float elapsedTime = 0f;
        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            SpeedCooldownBar.fillAmount = elapsedTime / cooldownTime; // 填充冷却条
            yield return null;
        }

        SpeedCooldownBar.fillAmount = 1; // 冷却完成
        SpeedReadyPrompt.SetActive(true); // 提示玩家按P激活技能
        isSpeedSkillReady = true;
    }

    private IEnumerator ActivateSpeedSkill()
    {
        isSpeedSkillReady = false;
        isSpeedActive = true;

        SpeedReadyPrompt.SetActive(false);
        SpeedSkillIconContainer.SetActive(false); // 隐藏技能图标
        SpeedSkillBarContainer.SetActive(true); // 显示技能条

        SpeedSkillBar.fillAmount = 1; // 技能条从0开始
        moveSpeed *= speedMultiplier; // 增加移动速度

        float elapsedTime = 0f;
        while (elapsedTime < speedDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = 1 - elapsedTime / speedDuration;
            SpeedSkillBar.fillAmount = progress; // 填充技能条
            UpdateSkillIconPosition(progress); // 更新图标位置
            yield return null;
        }

        moveSpeed /= speedMultiplier; // 恢复原始速度
        isSpeedActive = false;

        SpeedSkillBar.fillAmount = 1; // 技能条重置
        UpdateSkillIconPosition(0); // 重置图标位置

        SpeedSkillBarContainer.SetActive(false); // 隐藏技能条
        SpeedSkillIconContainer.SetActive(true); // 恢复显示技能图标

        StartCoroutine(SpeedCooldown()); // 开始冷却
    }

    private void UpdateSkillIconPosition(float progress)
    {
        SkillIcon.anchoredPosition = Vector2.Lerp(SkillIconLeftPosition, SkillIconRightPosition, progress);
    }
}
