using UnityEngine;

public class Banana : Entity
{
    // 香蕉不需要复杂的状态机，但我们为它创建一个以保持结构一致
    public Banana_IdleState idleState { get; private set; }

    protected override void Awake()
    {
        base.Awake(); // 调用父类的Awake来获取Animator和Rigidbody2D

        stateMachine = new StateMachine();
        idleState = new Banana_IdleState(this, stateMachine, "idle");
    }

    protected override void Start()
    {
        // 初始化状态机并进入Idle状态
        stateMachine.Initialize(idleState);
    }

    // 当有其他碰撞体进入香蕉的触发器时调用
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查碰到的是否是猴子 (请确保你的猴子对象的Tag是"Player")
        if (collision.CompareTag("Player"))
        {
            // 输出模拟成功的标志
            Debug.Log("SUCCESS! Monkey collected the banana!");

            // 销毁香蕉本身
            Destroy(gameObject);
        }
    }
}
