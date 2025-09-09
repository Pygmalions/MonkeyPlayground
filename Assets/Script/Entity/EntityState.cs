using UnityEngine;

public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolName;
    protected Animator anim;
    protected Rigidbody2D rb;

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }
    
    public virtual void Enter()
    {
        //进入状态时调用
        // Debug.Log("Enter " + stateName);
        anim.SetBool(animBoolName,true);
    }

    public virtual void Update()
    {
        //更行该状态时的逻辑
        // Debug.Log("Now in " + stateName);
    }

    public virtual void Exit()
    {
        //退出状态时调用
        // Debug.Log("Exit " + stateName);
        anim.SetBool(animBoolName,false);
    }
}
