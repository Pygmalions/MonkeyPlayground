using UnityEngine;

public class Monkey_MoveState : MonkeyState
{
    public Monkey_MoveState(Monkey monkey, StateMachine stateMachine, string animBoolName) : base(monkey, stateMachine, animBoolName)
    {
    }
    
    public override void Update()
    {
        base.Update();
        
        if (monkey.moveInput.x ==0)
            stateMachine.ChangeState(monkey.idleState);
        
        monkey.SetVelocity(monkey.moveInput.x * monkey.movespeed, rb.linearVelocity.y);
    }
}
