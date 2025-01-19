using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class DogController : MonoBehaviour
{
    public float moveSpeed = 3f; // 狗的移动速度
    private float originalMoveSpeed; // 保存原始的移动速度
    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    public float hideTimer = 5f; // 隐藏计时器
    public float forceRevealTimer = 1f; // 进度条清零导致的强制现身计时器
    public float bubbleForceRevealTimer = 2f; // 踏入隐藏泡泡导致的强制现身计时器
    private float currentHideTimer; // 当前隐藏计时器
    public bool isVisible = true; // 狗是否可见
    private bool isMoving = false; // 狗是否在移动
    private bool isForcingReveal = false; // 是否在强制现身状态
    private bool isSpeedReduced = false; // 是否处于速度减半状态

    public float stopBufferTime = 0.2f; // 缓冲时间，当狗速度接近零时使用
    private float stopBufferTimer = 0f; // 当前缓冲计时器

    public Image countdownBar;
    public Image countdownFrame; // 外框的 Image 组件

    public Color colorStage1; // 第一阶段颜色
    public Color colorStage2; // 第二阶段颜色
    public Color colorStage3; // 第三阶段颜色

    public Sprite frameStage1; // 第一阶段外框
    public Sprite frameStage2; // 第二阶段外框
    public Sprite frameStage3; // 第三阶段外框

    private Coroutine fillBarCoroutine; // 用于平滑回满的协程
    private QuickStrikeManager strikeSc;
    public GameObject dog2DObj;
    public AudioSource DogAppear, DogBreath;

    public Transform spriteTransform; // 子对象的Transform，用于翻转动画

    // 新增代码：Animator 组件
    public Animator animator; // Animator 组件

    [HideInInspector]
    public GameObject dogBreathVfx;
    [HideInInspector]
    public GameObject dogDisappearVfx;

    private void Awake()
    {
        dogBreathVfx = (GameObject)Resources.Load("VFX/DogBreath");
        dogDisappearVfx = (GameObject)Resources.Load("VFX/DogDisappear");
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        // 初始化状态
        SetVisibility(true);
        currentHideTimer = hideTimer; // 初始化隐藏计时器
        strikeSc = FindObjectOfType<QuickStrikeManager>();

        // 确保进度条初始为满
        if (countdownBar != null)
        {
            countdownBar.fillAmount = 1f;
        }

        // 如果没有手动设置 spriteTransform，可以尝试自动获取
        if (spriteTransform == null)
        {
            spriteTransform = transform.GetChild(0); // 假设子对象是第一个子物体
        }

        // 保存原始移动速度
        originalMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!strikeSc.isQuickStrikeActive)
        {
            HandleMovement();
            HandleHideTimer();
            UpdateCountdownBar();
        }
    }

    private void HandleMovement()
    {
        // 获取输入
        float moveX = Input.GetAxis("Horizontal"); // WASD
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized;
        rb.velocity = movement * moveSpeed;

        // 检查是否正在移动（速度是否大于某个阈值）
        if (movement.magnitude > 0.1f) // 如果狗的移动输入大于阈值
        {
            stopBufferTimer = 0f; // 重置缓冲计时器

            if (!isMoving)
            {
                isMoving = true;

                // 如果狗开始移动，隐藏狗并重置计时器
                if (!isForcingReveal)
                {
                    Instantiate(dogDisappearVfx, transform.position, Quaternion.identity);
                    SetVisibility(false);
                    currentHideTimer = hideTimer; // 重置隐藏计时器
                }
            }
        }
        else // 狗停止移动（速度接近零）
        {
            // 开始缓冲计时
            stopBufferTimer += Time.deltaTime;

            if (stopBufferTimer >= stopBufferTime) // 如果缓冲时间超过设定值
            {
                if (isMoving)
                {
                    isMoving = false;

                    // 如果狗停止移动且不在强制现身状态，则主动现身
                    if (!isForcingReveal)
                    {
                        Instantiate(dogBreathVfx, transform.position, Quaternion.identity);

                        // 插入狗甩干的声音
                        if (DogAppear != null)
                        {
                            if (!DogAppear.isPlaying)
                                DogAppear.Play();
                        }
                        SetVisibility(true);
                    }
                }
            }
        }

        // 新增代码：更新 Animator 参数
        if (animator != null)
        {
            Debug.Log("set animator IsStaying to  ----  " + !isMoving);
            animator.SetBool("IsStaying", !isMoving); // 如果狗在移动，设置 IsStaying 为 false；否则为 true
        }

        // 翻转动画
        if (moveX != 0) // 只有在水平移动时才翻转
        {
            Vector3 localScale = spriteTransform.localScale;
            localScale.x = moveX > 0 ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            spriteTransform.localScale = localScale;
        }
    }

    private void UpdateCountdownBar()
    {
        if (countdownBar != null)
        {
            if (!isVisible && !isForcingReveal) // 狗不可见且不在强制现身状态
            {
                countdownBar.fillAmount = currentHideTimer / hideTimer;

                // 根据进度条阶段设置颜色和外框
                float progress = countdownBar.fillAmount;

                if (progress > 2f / 3f) // 第一阶段
                {
                    countdownBar.color = colorStage1;
                    if (countdownFrame != null)
                    {
                        countdownFrame.sprite = frameStage1;
                    }
                }
                else if (progress > 1f / 3f) // 第二阶段
                {
                    countdownBar.color = colorStage2;
                }
                else // 第三阶段
                {
                    countdownBar.color = colorStage3;
                    if (countdownFrame != null)
                    {
                        countdownFrame.sprite = frameStage2;
                    }
                }
            }
            else if (isVisible) // 狗现身时
            {
                if (countdownFrame != null)
                {
                    countdownFrame.sprite = frameStage3;
                }

                if (fillBarCoroutine == null)
                {
                    fillBarCoroutine = StartCoroutine(SmoothFillBar());
                }
            }
            else if (isForcingReveal) // 如果狗正在强制现身，进度条保持空
            {
                countdownBar.fillAmount = 0f;
            }
        }
    }

    private System.Collections.IEnumerator SmoothFillBar()
    {
        float duration = 0.5f; // 平滑填充的时间
        float startFill = countdownBar.fillAmount; // 当前填充值
        float targetFill = 1f; // 目标填充值
        float elapsedTime = 0f;

        countdownBar.color = colorStage1;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            countdownBar.fillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / duration);
            yield return null;
        }

        countdownBar.fillAmount = targetFill; // 确保最终填充为满
        fillBarCoroutine = null; // 重置协程引用
    }

    public System.Collections.IEnumerator ForceReveal(float revealDuration, bool triggeredByProgressBar = false)
    {
        if (isForcingReveal) yield break;

        isForcingReveal = true;
        Debug.Log("Dog is forced to reveal!");
        Instantiate(dogBreathVfx, transform.position, Quaternion.identity);
        SetVisibility(true);

        // 减速逻辑仅在进度条为零时触发
        if (triggeredByProgressBar)
        {
            isSpeedReduced = true;
            moveSpeed = originalMoveSpeed / 2;
        }

        yield return new WaitForSeconds(revealDuration);

        isForcingReveal = false;

        // 恢复速度
        if (triggeredByProgressBar)
        {
            isSpeedReduced = false;
            moveSpeed = originalMoveSpeed;
        }

        if (isMoving)
        {
            Instantiate(dogDisappearVfx, transform.position, Quaternion.identity);
            SetVisibility(false);
            currentHideTimer = hideTimer;
        }
        else
        {
            Instantiate(dogBreathVfx, transform.position, Quaternion.identity);
            SetVisibility(true);
        }
    }

    private void HandleHideTimer()
    {
        if (!isVisible && !isForcingReveal)
        {
            currentHideTimer -= Time.deltaTime;
            if (currentHideTimer <= 0)
            {
                // 当进度条清零时，触发 ForceReveal，并传递 triggeredByProgressBar 为 true
                StartCoroutine(ForceReveal(forceRevealTimer, true));
            }
        }
    }


    private void SetVisibility(bool visible)
    {
        if (isForcingReveal && !visible) return;

        bool wasVisible = isVisible;
        isVisible = visible;

        if (dog2DObj != null)
        {
            dog2DObj.gameObject.SetActive(visible);
        }

        if (visible && !wasVisible)
        {
            NotifyNearbyBubbles();
        }

        if (visible && countdownBar != null && fillBarCoroutine == null)
        {
            fillBarCoroutine = StartCoroutine(SmoothFillBar());
        }

        Debug.Log($"Dog visibility set to: {visible}");
    }

    private void NotifyNearbyBubbles()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, .35f);
        foreach (Collider collider in nearbyColliders)
        {
            if (collider.TryGetComponent(out BubbleController bubble))
            {
                bubble.OnTriggerEnter(GetComponent<Collider>());
                bubble.CheckEntityStatus();
            }
        }
    }
}
