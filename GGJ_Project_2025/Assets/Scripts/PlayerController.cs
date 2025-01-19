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
    public bool canMove = true; // 是否可以移动（用于滑倒时禁用移动）
    public bool isSpeedSkillUnlocked = false; // 是否解锁了加速技能
    private bool isSpeedSkillReady = false; // 是否可以激活加速技能
    private bool isSpeedActive = false; // 是否正在使用加速技能
    private QuickStrikeManager strikeSc;
    public Transform spriteTransform; // 子对象的Transform，用于翻转动画
    public AudioSource slipsound, SpeedUp;

    public GameObject WarningObject; // 用于显示警告的对象
    public Transform dog; // 狗的 Transform
    private bool isWarningActive = false; // 警告是否激活

    public Animator animator; // 自定义的Animator

    [HideInInspector]
    public GameObject accelerateVfx;
    [HideInInspector]
    public GameObject soapVfx;
    [HideInInspector]
    public GameObject catchVfx;

    private void Awake()
    {
        accelerateVfx = (GameObject)Resources.Load("VFX/Accelerate");
        soapVfx = (GameObject)Resources.Load("VFX/StepOnSoapTwo");
        catchVfx = (GameObject)Resources.Load("VFX/CatchMoment");
    }

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

        // 初始化警告对象为隐藏状态
        WarningObject.SetActive(false);
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

        // 检测狗是否进入或离开范围
        CheckDogDistance();
    }

    void Move()
    {
        // 仅当 canMove 为 true 时允许移动
        if (!canMove)
        {
            rb.velocity = Vector3.zero; // 确保角色停止移动
            return;
        }

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

        // 更新动画状态
        if (animator != null)
        {
            animator.SetBool("IsWalking", moveDirection.magnitude > 0);
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
            Instantiate(catchVfx, transform.position, Quaternion.identity);
            Debug.Log("Catch the dog!");

            // 如果加速技能正在激活，提前停止
            if (isSpeedActive)
            {
                StopCoroutine("ActivateSpeedSkill"); // 停止加速协程
                moveSpeed /= speedMultiplier; // 恢复原始速度
                isSpeedActive = false;
                SpeedSkillBarContainer.SetActive(false);
                SpeedSkillIconContainer.SetActive(true);

                // 恢复Animator的SpeedUp为false
                if (animator != null)
                {
                    animator.SetBool("SpeedUp", false);
                }
            }

            // 确保冷却协程重新启动
            RestartSpeedCooldown();

            // 开始QuickStrike逻辑
            strikeSc.StartQuickStrike();
        }
    }


    private IEnumerator Slip()
    {
        Instantiate(soapVfx, transform.position, Quaternion.identity);
        if (!slipsound.isPlaying)
            slipsound.Play();

        // 如果正在加速，停止加速逻辑并开始冷却
        if (isSpeedActive)
        {
            StopCoroutine("ActivateSpeedSkill"); // 停止加速协程
            moveSpeed /= speedMultiplier; // 恢复原始速度
            isSpeedActive = false;
            SpeedSkillBarContainer.SetActive(false); // 隐藏技能条
            SpeedSkillIconContainer.SetActive(true); // 显示技能图标

            // 恢复Animator的SpeedUp为false
            if (animator != null)
            {
                animator.SetBool("SpeedUp", false);
            }
        }

        // 触发Animator中的Slip Trigger
        if (animator != null)
        {
            animator.SetTrigger("Slip");
        }

        // 禁用移动
        canMove = false;
        SlipBar.SetActive(true);
        rb.velocity = Vector3.zero; // 停止移动
        yield return new WaitForSeconds(slipTime); // 滑倒持续指定时间

        // 恢复移动权限
        // 重置加速冷却
        RestartSpeedCooldown();
        canMove = true;
        SlipBar.SetActive(false);

        // 恢复Animator到行走状态
        if (animator != null)
        {
            animator.ResetTrigger("Slip"); // 重置“滑倒”触发器
            animator.SetBool("IsWalking", true); // 设置为行走状态
        }
    }



    private void UnlockSpeedSkill()
    {
        isSpeedSkillUnlocked = true;
        SpeedSkillIconContainer.SetActive(true); // 显示技能图标
        StartCoroutine(SpeedCooldown());
    }

    private IEnumerator SpeedCooldown()
    {
        Debug.Log("Speed Cool down start");
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
        // 如果冷却协程正在运行，停止它
        StopCoroutine("SpeedCooldown");

        isSpeedSkillReady = false;
        isSpeedActive = true;

        SpeedReadyPrompt.SetActive(false);
        SpeedSkillIconContainer.SetActive(false); // 隐藏技能图标
        SpeedSkillBarContainer.SetActive(true); // 显示技能条

        SpeedSkillBar.fillAmount = 1; // 技能条从0开始
        moveSpeed *= speedMultiplier; // 增加移动速度

        Instantiate(accelerateVfx, transform.position, Quaternion.identity);
        // 播放加速音效
        if (!SpeedUp.isPlaying)
            SpeedUp.Play();

        // **设置Animator的SpeedUp为true**
        if (animator != null)
        {
            animator.SetBool("SpeedUp", true);
        }

        float elapsedTime = 0f;
        while (elapsedTime < speedDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = 1 - elapsedTime / speedDuration;
            SpeedSkillBar.fillAmount = progress; // 填充技能条
            yield return null;
        }

        moveSpeed /= speedMultiplier; // 恢复原始速度
        isSpeedActive = false;
        SpeedSkillBarContainer.SetActive(false); // 隐藏技能条
        SpeedSkillIconContainer.SetActive(true); // 显示技能图标

        // **恢复Animator的SpeedUp为false**
        if (animator != null)
        {
            animator.SetBool("SpeedUp", false);
        }

        StartCoroutine(SpeedCooldown()); // 开始冷却
    }

    public void RestartSpeedCooldown()
    {
        // 如果加速技能已经解锁但未准备好，则重新启动冷却
        if (isSpeedSkillUnlocked)
        {
            StopAllCoroutines(); // 停止所有协程，避免重复
            StartCoroutine(SpeedCooldown());
        }
    }


    private void CheckDogDistance()
    {
        float distance = Vector3.Distance(transform.position, dog.position);

        // 如果狗进入3米范围
        if (distance <= 3f)
        {
            if (!isWarningActive)
            {
                WarningObject.SetActive(true);
                isWarningActive = true;
            }
        }
        // 如果狗离开3米范围
        else
        {
            if (isWarningActive)
            {
                WarningObject.SetActive(false);
                isWarningActive = false;
            }
        }
    }

    private void UpdateSkillIconPosition(float progress)
    {
        SkillIcon.anchoredPosition = Vector2.Lerp(SkillIconLeftPosition, SkillIconRightPosition, progress);
    }
}