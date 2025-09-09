using UnityEngine;

// 这是所有香蕉状态的抽象基类
public abstract class BananaState : EntityState
{
    protected Banana banana;

    public BananaState(Banana banana, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.banana = banana;
        this.anim = banana.anim; // 从Banana实体获取Animator
        this.rb = banana.rb;     // 从Banana实体获取Rigidbody2D
    }
}