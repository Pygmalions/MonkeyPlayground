using System;
using JetBrains.Annotations;
using UnityEngine;

public class Box : Entity
{
    private Monkey currentMonkey; // 用于存储进入触发范围的猴子
    [SerializeField] private bool isBeingCarried = false;
    
    private bool isBeingSteppedOn = false;
    
    public bool IsBeingCarried
    {
        get { return isBeingCarried; }
    }
    
    public bool IsBeingSteppedOn
    {
        get { return isBeingSteppedOn; }
    }

    protected override void Awake()
    {
        base.Awake();
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞的是否是猴子
        if (collision.gameObject.CompareTag("Player"))
        {
            // 获取碰撞点信息
            ContactPoint2D contact = collision.GetContact(0);

            // 检查猴子是否从上方踩下来
            // contact.normal.y < -0.5f 表示对方是从正上方附近撞过来的
            if (contact.normal.y < -0.5f)
            {
                isBeingSteppedOn = true;
                Debug.Log("箱子被踩踏了！"); // 用于测试的日志
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // 检查离开的是否是猴子
        if (collision.gameObject.CompareTag("Player"))
        {
            // 只要猴子离开了，就认为不再被踩踏
            isBeingSteppedOn = false;
            Debug.Log("猴子离开了箱子。"); // 用于测试的日志
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentMonkey = collision.GetComponent<Monkey>();
            if (currentMonkey != null)
            {
                // --- 修改代码: 调用新的方法 ---
                currentMonkey.AddInteractableBox(this);
                // --- 修改代码结束 ---
                Debug.Log("Entered box interaction zone. Press 'E' to Climb or 'F' to Transport");
            }
        }
    }

    // 当有物体离开Box的触发器时，这个方法会被调用
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentMonkey != null)
            {
                // --- 修改代码: 调用新的方法 ---
                currentMonkey.RemoveInteractableBox(this);
                // --- 修改代码结束 ---
                currentMonkey = null;
                Debug.Log("Exited box interaction zone.");
            }
        }
    }

    public struct ActionResult
    {
        public bool Succeeded;

        [CanBeNull] public string Message;
    }
    
    public ActionResult Climb(Transform monkeyTransform)
    {
        // 将猴子移动到箱子的正上方
        float boxHeight = GetComponent<Collider2D>().bounds.size.y;
        float monkeyHeight = monkeyTransform.GetComponent<Collider2D>().bounds.size.y;
        monkeyTransform.position = new Vector3(transform.position.x, transform.position.y + monkeyHeight / 2 + boxHeight/2, monkeyTransform.position.z);
        return new ActionResult()
        {
            Succeeded = true
        };
    }
    
    public void Pickup(Transform monkeyTransform)
    {
        isBeingCarried = true; // 标记为正在被搬运
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        rb.bodyType = RigidbodyType2D.Kinematic; // 在搬运时临时切换为 Kinematic，避免物理抖动
        rb.linearVelocity = Vector2.zero; // 清除速度
        
        transform.SetParent(monkeyTransform);
        // 调整箱子的位置，使其在猴子的正上方
        float boxHeight = GetComponent<Collider2D>().bounds.size.y;
        float monkeyHeight = monkeyTransform.GetComponent<Collider2D>().bounds.size.y;
        transform.localPosition = new Vector3(0, boxHeight / 2 + monkeyHeight, 0);
    }
    
    public void PutDown(Monkey monkey)
    {
        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;
        isBeingCarried = false;

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        
        // 判断猴子的朝向，决定箱子放在左边还是右边
        float dropOffset = 1.0f; // 可调整的放下距离
        int facingDirection = monkey.IsFacingRight ? 1 : -1;
        
        // 计算放下位置 (在猴子面前)
        Vector3 dropPosition = monkey.transform.position + new Vector3(dropOffset * facingDirection, 0, 0);

        // 将箱子放在计算好的位置
        transform.position = dropPosition;
    }
}