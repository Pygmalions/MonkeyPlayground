using System;
using Unity.VisualScripting;
using System.Collections.Generic; // 需要引入这个命名空间来使用 List
using System.Linq; // 需要引入这个来使用 Linq 查询
using UnityEngine;

public class Monkey : Entity
{
    private MonkeyInputSet input;

    public Monkey_IdleState idleState{ get; private set; }
    public Monkey_MoveState moveState{ get; private set; }

    
    private List<Box> interactableBoxes = new List<Box>(); // 存储所有在交互范围内的箱子
    private Box carriedBox; // 当前正在搬运的箱子
    
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
        base.Update();
        
        HandleInteraction();
    }   
    
    private void HandleInteraction()
    {
        Box closestBox = GetClosestInteractableBox();

        // 1. 处理放下 ('G'键)
        if (Input.GetKeyDown(KeyCode.G) && carriedBox != null)
        {
            // --- 修改: 传递整个 Monkey 对象 ---
            carriedBox.PutDown(this);
            carriedBox = null;
            return;
        }

        if (closestBox == null) return;

        // 2. 处理攀爬 ('E'键)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // --- 核心修改: 移除放下箱子的代码 ---
            // 现在猴子会直接带着箱子攀爬
            closestBox.Climb(transform);
        }
        // 3. 处理搬运 ('F'键)
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (carriedBox == null)
            {
                carriedBox = closestBox;
                carriedBox.Pickup(transform);
            }
        }
    }
    
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
