using UnityEngine;

public class DogController : MonoBehaviour
{
    public float moveSpeed = 6f;          // 狗的移动速度
    public float revealDuration = 1f;    // 每次现身持续时间
    public float hiddenInterval = 5f;    // 隐藏状态下的计时间隔

    private Rigidbody rb;                // 狗的刚体组件
    private MeshRenderer meshRenderer;   // 狗的MeshRenderer组件
    private float timer = 0f;            // 计时器（共享主动和被动现身）
    private bool isHidden = true;        // 是否处于隐藏状态
    private bool isRevealing = false;    // 是否正在现身
    private bool isPassiveReveal = false; // 是否是被动现身

    void Start()
    {
        // 获取刚体和MeshRenderer组件
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (rb == null || meshRenderer == null)
        {
            Debug.LogError("Missing Rigidbody or MeshRenderer component!");
        }
    }

    void Update()
    {
        Move();
        HandleVisibility();
    }

    void Move()
    {
        // 获取方向键输入
        float moveX = Input.GetAxis("HorizontalArrow"); // ←/→键
        float moveZ = Input.GetAxis("VerticalArrow");   // ↑/↓键

        // 设置刚体速度
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        rb.velocity = moveDirection * moveSpeed;
    }

    void HandleVisibility()
    {
        // 检查狗是否在移动
        bool isMoving = rb.velocity.magnitude > 0.1f;

        // 如果狗正在现身
        if (isRevealing)
        {
            timer += Time.deltaTime;

            // 如果是被动现身，现身期间不受移动影响
            if (isPassiveReveal)
            {
                if (timer >= revealDuration)
                {
                    SetHidden(true); // 被动现身时间结束后隐藏
                }
            }
            else
            {
                // 如果是主动现身，移动时立即隐藏
                if (isMoving)
                {
                    SetHidden(true);
                }
                else if (timer >= revealDuration)
                {
                    SetHidden(true); // 主动现身时间结束后隐藏
                }
            }

            return; // 现身期间不执行其他逻辑
        }

        // 如果狗停止运动且当前隐藏，主动现身
        if (!isMoving && isHidden)
        {
            Reveal(false); // 主动现身
            return;
        }

        // 如果狗在移动，累加计时器
        if (isMoving)
        {
            timer += Time.deltaTime;
            if (timer >= hiddenInterval)
            {
                Reveal(true); // 被动现身
            }
        }
    }

    void Reveal(bool passive)
    {
        SetHidden(false); // 现身
        isRevealing = true;
        isPassiveReveal = passive; // 标记是否为被动现身
        timer = 0f; // 重置计时器
    }

    void SetHidden(bool hide)
    {
        isHidden = hide;
        isRevealing = !hide;
        meshRenderer.enabled = !hide; // 隐藏或显示狗的模型

        // 如果隐藏，则重新开始计时
        if (hide)
        {
            timer = 0f;
        }
    }
}


