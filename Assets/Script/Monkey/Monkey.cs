using System;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic; 
using System.Linq; 
using UnityEngine;

public class Monkey : Entity
{
    private MonkeyInputSet input;

    public Monkey_IdleState idleState{ get; private set; }
    public Monkey_MoveState moveState{ get; private set; }
    
    public bool isUnderCoroutineControl = false;
    
    private List<Box> interactableBoxes = new List<Box>(); // 存储所有在交互范围内的箱子
    private Box carriedBox; // 当前正在搬运的箱子
    
    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer; // 请在Unity Inspector中设置此图层
    
    public bool IsFacingRight { get { return facingRight; } }

    [Header("Movement Details")] 
    public float movespeed;
    
    public Vector2 moveInput;

    protected override void Awake()
    {
        base.Awake();
                
        stateMachine = new StateMachine();
        input = new MonkeyInputSet();
        
        idleState = new Monkey_IdleState(this,stateMachine, "idle");
        moveState = new Monkey_MoveState(this,stateMachine, "move");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        if (! isUnderCoroutineControl)
        {
            stateMachine.UpdateActiveState();
            HandleInteraction();
        }
    }   
    
    private void HandleInteraction()
    {
        Box closestBox = GetClosestInteractableBox();
        Box highestBox = GetHighestClimbableBox();

        // 1. 处理放下 ('G'键)
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryPutDownBox();
        }
        
        // 2. 处理攀爬 ('E'键)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryClimbBox();
        }
        // 3. 处理搬运 ('F'键)
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupBox();
        }
    }

        
    #region Monkey Action
    private ActionResult TryPutDownBox()
    {
        ActionResult result;
        if (carriedBox != null)
        {
            result = carriedBox.PutDown(this);
            if (result.Succeeded)
            {
                carriedBox = null;
            }
        }
        else
        {
            Debug.Log("没有搬运箱子");
            result = ActionResult.Failure("没有搬运箱子");
        }

        return result;
    }

    private ActionResult TryClimbBox()
    {
        Box closestBox = GetClosestInteractableBox();
        Box highestBox = GetHighestClimbableBox();
        ActionResult result;

        if (highestBox != null)
        {
            result = highestBox.Climb(transform);
        }
        else if (closestBox != null)
        {
            result = closestBox.Climb(transform);
        }
        else
        {
            Debug.Log("附近没有箱子");
            result =  ActionResult.Failure("附近没有箱子");
        }

        return result;
    }

    private ActionResult TryPickupBox()
    {
        Box closestBox = GetClosestInteractableBox();
        ActionResult result;
        
        if (closestBox == null) 
        {
            result = ActionResult.Failure("附近没有箱子");
        }
        else if (carriedBox == null)
        {
            carriedBox = closestBox;
            result = carriedBox.Pickup(this);
        }
        else
        {
            result = ActionResult.Failure("已经搬运了箱子");
        }

        return result;
    }
    
    public void MoveHorizontally(float distance, Action<ActionResult> onComplete)
    {
        StartCoroutine(MoveHorizontallyCoroutine(distance, onComplete));
    }

    private IEnumerator MoveHorizontallyCoroutine(float distance, Action<ActionResult> onComplete)
    {
        isUnderCoroutineControl = true; // 暂停状态机和玩家输入
        anim.SetBool("move", true);     // 直接命令Animator播放“行走”动画
        anim.SetBool("idle", false);    // 确保“站立”动画被关闭

        float direction = Mathf.Sign(distance);
        Vector3 targetPosition = transform.position + new Vector3(distance, 0, 0);
        Collider2D monkeyCollider = GetComponent<Collider2D>();
        float raycastDistance = monkeyCollider.bounds.size.x * 0.5f + 0.1f;

        while (Mathf.Abs(targetPosition.x - transform.position.x) > 0.05f)
        {
            Vector2 rayOrigin = new Vector2(transform.position.x, monkeyCollider.bounds.center.y);
            Vector2 rayDirection = new Vector2(direction, 0);
            Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance, obstacleLayer);
            if (hit.collider != null)
            {
                Debug.Log("存在障碍物: " + hit.collider.name);
                SetVelocity(0, rb.linearVelocity.y);
                
                isUnderCoroutineControl = false;      // 恢复状态机和玩家输入
                anim.SetBool("move", false);          // 停止“行走”动画
                stateMachine.ChangeState(idleState); // 确保状态机回到Idle状态
                
                onComplete?.Invoke(ActionResult.Failure($"移动被障碍物 {hit.collider.name} 阻挡"));
                yield break;
            }

            SetVelocity(movespeed * direction, rb.linearVelocity.y);

            if ((direction > 0 && transform.position.x > targetPosition.x) || (direction < 0 && transform.position.x < targetPosition.x))
            {
                break;
            }
            yield return null;
        }

        SetVelocity(0, rb.linearVelocity.y);
        transform.position = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
        
        isUnderCoroutineControl = false;      // 恢复状态机和玩家输入
        anim.SetBool("move", false);          // 停止“行走”动画
        stateMachine.ChangeState(idleState); // 确保状态机回到Idle状态

        onComplete?.Invoke(ActionResult.Success("移动成功完成"));
    }
    
    #endregion
    
    
    # region About boxes
    private Box GetClosestInteractableBox()
    {
        Box closest = null;
        float minDistance = float.MaxValue;

        foreach (var box in interactableBoxes)
        {
            // 跳过正在搬运的箱子
            if (box == carriedBox)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, box.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = box;
            }
        }
        return closest;
    }    
    
    private Box GetHighestClimbableBox()
    {
        Box highestClimbableBox = null;
        float maxHeight = float.MinValue; // 用于记录当前找到的最高的可攀爬箱子的顶部高度

        // 为了进行高度差计算，我们首先需要获取猴子自己的碰撞体
        Collider2D monkeyCollider = GetComponent<Collider2D>();
        if (monkeyCollider == null)
        {
            Debug.LogError("Monkey身上没有找到Collider2D！");
            return null; // 没有碰撞体则无法计算，直接返回
        }
    
        // 获取猴子脚底的Y坐标，这个值在整个循环中是固定的
        float monkeyFeetY = monkeyCollider.bounds.min.y;

        foreach (var box in interactableBoxes)
        {
            // 1. 跳过正在搬运的箱子
            if (box == carriedBox)
            {
                continue;
            }

            // 2. 检查这个箱子是否可以攀爬
            Collider2D boxCollider = box.GetComponent<Collider2D>();
            if (boxCollider == null) continue; // 如果箱子没有碰撞体，跳过

            float boxTopY = boxCollider.bounds.max.y;
            float heightDifference = boxTopY - monkeyFeetY;

            // 如果高度差大于1，说明太高了爬不上去，直接跳过这个箱子
            if (heightDifference > 1f)
            {
                continue;
            }
        
            // 3. 如果这个箱子可以爬，并且它的顶部比我们之前找到的还要高
            if (boxTopY > maxHeight)
            {
                maxHeight = boxTopY;         // 更新最大高度
                highestClimbableBox = box; // 将这个箱子设为新的候选者
            }
        }

        return highestClimbableBox; // 返回找到的最高的、且能爬上去的箱子
    }
    
    public void AddInteractableBox(Box box)
    {
        if (!interactableBoxes.Contains(box))
        {
            interactableBoxes.Add(box);
        }
    }

    public void RemoveInteractableBox(Box box)
    {
        if (interactableBoxes.Contains(box))
        {
            interactableBoxes.Remove(box);
        }
    }
    #endregion
    
    private void OnEnable()
    {
        input.Enable();

        input.Monkey.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Monkey.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }
    
    
}
