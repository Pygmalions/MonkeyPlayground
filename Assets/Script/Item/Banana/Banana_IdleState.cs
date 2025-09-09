using UnityEngine;

public class Banana_IdleState : BananaState
{
    public Banana_IdleState(Banana banana, StateMachine stateMachine, string animBoolName) : base(banana, stateMachine, animBoolName)
    {
    }

    // 因为香蕉的Idle状态不需要做任何逻辑，
    // 所以我们把Enter, Update, Exit方法留空，
    // 继承自基类的Enter/Exit方法会自动处理动画布尔值的切换。
}