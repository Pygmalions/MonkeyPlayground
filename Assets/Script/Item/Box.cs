using UnityEngine;

public class Box : Entity
{
    private Monkey currentMonkey; // 用于存储进入触发范围的猴子
    [SerializeField] private bool isBeingCarried = false;
    
    [Header("放置设置")]
    [Tooltip("定义哪些图层是障碍物")]
    [SerializeField] private LayerMask obstacleLayer;
    
    // 我们需要箱子自己的实体碰撞体
    private BoxCollider2D boxCollider;
    // 以及猴子的碰撞体
    private Collider2D monkeyCollider;
    
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
        // 获取实体碰撞体（请确保 IsTrigger 没有被勾选）
        boxCollider = GetComponent<BoxCollider2D>();
    }
    
    // 【核心逻辑】在Update中持续判断
    protected override void Update()
    {
        base.Update();
        
        // 确保猴子在附近且我们有它的碰撞体，并且箱子没被举起
        if (currentMonkey == null || monkeyCollider == null || isBeingCarried)
        {
            return;
        }

        // 获取猴子脚底的Y轴位置
        float monkeyFeetY = monkeyCollider.bounds.min.y;
        // 获取箱子顶部的Y轴位置
        float boxTopY = boxCollider.bounds.max.y;

        // 【关键判断】
        // 如果猴子的脚低于箱子顶部（允许误差），则忽略碰撞，让猴子可以穿过
        if (monkeyFeetY < boxTopY - 0.05f)
        {
            Physics2D.IgnoreCollision(boxCollider, monkeyCollider, true);
        }
        else
        {
            // 否则，当猴子在箱子上方时，启用碰撞，让猴子可以站立
            Physics2D.IgnoreCollision(boxCollider, monkeyCollider, false);
        }
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            if (contact.normal.y < -0.5f)
            {
                isBeingSteppedOn = true;
                Debug.Log("箱子被踩踏了！");
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isBeingSteppedOn = false;
            Debug.Log("猴子离开了箱子。");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentMonkey = collision.GetComponent<Monkey>();
            if (currentMonkey != null)
            {
                // 在进入触发器时，安全地获取猴子的碰撞体
                monkeyCollider = collision.GetComponent<Collider2D>();
                
                currentMonkey.AddInteractableBox(this);
                Debug.Log("Entered box interaction zone.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (currentMonkey != null)
            {
                // 在猴子离开时，务必恢复碰撞设置
                if (monkeyCollider != null)
                {
                    Physics2D.IgnoreCollision(boxCollider, monkeyCollider, false);
                }
                
                // 清空所有引用
                monkeyCollider = null;
                currentMonkey.RemoveInteractableBox(this);
                currentMonkey = null;
                Debug.Log("Exited box interaction zone.");
            }
        }
    }
    
    public ActionResult Climb(Transform monkeyTransform)
    {
        Collider2D monkeyCollider = monkeyTransform.GetComponent<Collider2D>();
        Collider2D boxCollider = GetComponent<Collider2D>();
        if (monkeyCollider == null)
        {
            Debug.LogWarning("猴子碰撞体读取错误");
        }
        float monkeyFeetY = monkeyCollider.bounds.min.y;
        float boxTopY = boxCollider.bounds.max.y;
        float heightDifference = boxTopY - monkeyFeetY;
        if (heightDifference > 1f)
        {
            Debug.Log($"攀爬失败: 高度差 {heightDifference} > 1");
            return ActionResult.Failure("离箱顶太远了，爬不上去！");
        }
        
        float boxHeight = boxCollider.bounds.size.y;
        float monkeyHeight = monkeyCollider.bounds.size.y;
        monkeyTransform.position = new Vector3(transform.position.x, transform.position.y + boxHeight / 2, monkeyTransform.position.z);
    
        return ActionResult.Success();
    }
    
    public ActionResult Pickup(Monkey monkey)
    {
        isBeingCarried = true;
        Transform monkeyTransform = monkey.transform;
        float dropOffset = 1.0f;
        int facingDirection = monkey.IsFacingRight ? 1 : -1;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.SetParent(monkeyTransform);
        float boxHeight = GetComponent<Collider2D>().bounds.size.y;
        float boxWidth = GetComponent<Collider2D>().bounds.size.x;
        float monkeyHeight = monkeyTransform.GetComponent<Collider2D>().bounds.size.y;
        transform.localPosition = new Vector3(dropOffset * boxWidth / 2,boxHeight / 2, 0);
        return ActionResult.Success();
    }

    public ActionResult PutDown(Monkey monkey)
    {
        float dropOffset = 1.0f;
        float boxHeight = GetComponent<Collider2D>().bounds.size.y;
        float boxWidth = GetComponent<Collider2D>().bounds.size.x;
        int facingDirection = monkey.IsFacingRight ? 1 : -1;
        Vector3 dropPosition = monkey.transform.position + new Vector3(dropOffset* boxWidth / 2 * facingDirection,boxHeight / 2, 0);

        Collider2D overlapCheck = Physics2D.OverlapBox(dropPosition, boxCollider.size * 0.8f, 0, obstacleLayer);

        if (overlapCheck != null)
        {
            Debug.Log("放置失败：目标位置被障碍物阻挡！");
            return ActionResult.Failure("放置失败：目标位置被障碍物阻挡！");
        }

        transform.SetParent(null);
        transform.position = dropPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;
        isBeingCarried = false;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        return ActionResult.Success();
    }
}