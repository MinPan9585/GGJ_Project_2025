using UnityEngine;

public class DogController : MonoBehaviour
{
    public float moveSpeed = 3f; // 狗的移动速度
    private Rigidbody rb;
    private MeshRenderer meshRenderer;

    public float hideTimer = 5f; // 隐藏计时器
    public float forceRevealTimer = 1f; // 强制现身的计时器
    private float currentHideTimer; // 当前隐藏计时器
    public bool isVisible = true; // 狗是否可见
    private bool isMoving = false; // 狗是否在移动
    private bool isForcingReveal = false; // 是否在强制现身状态

    public float stopBufferTime = 0.2f; // 缓冲时间，当狗速度接近零时使用
    private float stopBufferTimer = 0f; // 当前缓冲计时器

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        // 初始化状态
        SetVisibility(true);
        currentHideTimer = hideTimer; // 初始化隐藏计时器
    }

    private void Update()
    {
        HandleMovement();
        HandleHideTimer();
    }

    private void HandleMovement()
    {
        // 获取输入
        float moveX = Input.GetAxis("HorizontalArrow");
        float moveZ = Input.GetAxis("VerticalArrow");

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
                        SetVisibility(true);
                    }
                }
            }
        }
    }

    private void HandleHideTimer()
    {
        // 如果狗不可见且不在强制现身状态，开始倒计时
        if (!isVisible && !isForcingReveal)
        {
            currentHideTimer -= Time.deltaTime;
            if (currentHideTimer <= 0)
            {
                StartCoroutine(ForceReveal());
            }
        }
    }

    public System.Collections.IEnumerator ForceReveal()
    {
        if (isForcingReveal) yield break; // 防止重复调用

        isForcingReveal = true; // 标记为强制现身状态
        Debug.Log("Dog is forced to reveal!"); // 调试输出

        SetVisibility(true); // 强制现身

        // 等待强制现身计时器完成
        yield return new WaitForSeconds(forceRevealTimer);

        // 强制现身结束后
        isForcingReveal = false; // 退出强制现身状态

        if (isMoving) // 如果狗正在移动
        {
            SetVisibility(false); // 重新隐藏狗
            currentHideTimer = hideTimer; // 重置隐藏计时器
        }
        else // 如果狗未移动
        {
            SetVisibility(true); // 保持现身状态
        }
    }

    private void SetVisibility(bool visible)
    {
        // 如果狗正在强制现身，不允许其他逻辑覆盖可见性
        if (isForcingReveal && !visible) return;

        bool wasVisible = isVisible; // 记录之前的状态
        isVisible = visible;
        meshRenderer.enabled = visible;
        Debug.Log($"Dog visibility set to: {visible}"); // 调试输出

        // 通知所有触发区域内的泡泡重新检测状态
        NotifyNearbyBubbles(wasVisible);
    }

    private void NotifyNearbyBubbles(bool wasVisible)
    {
        float detectionRadius = 0.8f; // 调整检测范围，确保覆盖泡泡的触发范围
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        //Debug.Log($"Detected {colliders.Length} bubbles in range."); // 打印检测到的泡泡数量

        foreach (Collider collider in colliders)
        {
            GameObject obj = collider.gameObject;
            if (obj.CompareTag("Bubble"))
            {
                BubbleController bubble = obj.GetComponent<BubbleController>();
                if (bubble != null)
                {
                    // 如果狗从隐身变为现身，主动通知泡泡
                    if (!wasVisible && isVisible)
                    {
                        bubble.OnTriggerEnter(GetComponent<Collider>());
                    }

                    // 通知泡泡重新检测状态
                    bubble.CheckEntityStatus();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bubble"))
        {
            BubbleController bubble = other.GetComponent<BubbleController>();
            if (bubble != null)
            {
                // 如果泡泡已经隐藏，且狗是隐身状态，则强制现身
                if (!bubble.IsVisible() && !isVisible && !isForcingReveal)
                {
                    StartCoroutine(ForceReveal());
                }
            }
        }
    }
}
