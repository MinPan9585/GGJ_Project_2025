using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 主人移动速度
    private Rigidbody rb;       // 主人的刚体
    private bool canMove = true; // 是否可以移动（用于滑倒时禁用移动）

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (canMove)
        {
            Move();
        }
    }

    void Move()
    {
        // 获取WASD输入
        float moveX = Input.GetAxis("Horizontal"); // A/D键
        float moveZ = Input.GetAxis("Vertical");   // W/S键

        // 设置刚体速度
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        rb.velocity = moveDirection * moveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 检测与肥皂的碰撞
        if (collision.gameObject.CompareTag("Soap"))
        {
            StartCoroutine(Slip());
        }
    }

    private IEnumerator Slip()
    {
        canMove = false; // 禁用移动
        rb.velocity = Vector3.zero; // 停止移动
        yield return new WaitForSeconds(1f); // 滑倒持续1秒
        canMove = true; // 恢复移动
    }
}


