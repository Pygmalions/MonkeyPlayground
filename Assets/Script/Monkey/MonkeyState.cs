using UnityEngine;

public abstract class MonkeyState : EntityState
{
    protected Monkey monkey;
    

    public MonkeyState(Monkey monkey,StateMachine stateMachine, string animBoolName):base(stateMachine,animBoolName)
    {
        this.monkey = monkey;

        anim = monkey.anim;
        rb = monkey.rb;
    }

    public override void Enter()
    {
        base.Enter();
    }

    override public void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}