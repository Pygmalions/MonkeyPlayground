using UnityEngine;

public class Monkey_IdleState : MonkeyState
{
    public Monkey_IdleState(Monkey monkey,StateMachine stateMachine, string animBoolName) : base(monkey, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        
        if (monkey.moveInput.x !=0)
            stateMachine.ChangeState(monkey.moveState);
    }
}
