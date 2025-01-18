using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 主人移动速度
    public float slipTime = 1f;
    public GameObject SlipBar;
    private Rigidbody rb;       // 主人的刚体
    private bool canMove = true; // 是否可以移动（用于滑倒时禁用移动）
    public Transform spriteTransform; // 子对象的Transform，用于翻转动画
    private QuickStrikeManager strikeSc;

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
    }

    void Update()
    {
        if (canMove && !strikeSc.isQuickStrikeActive)
        {
            Move();
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
        //检测与肥皂的碰撞
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
            //FindObjectOfType<GameManager>().EndGame(true); // 主人胜利
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
    }
}



