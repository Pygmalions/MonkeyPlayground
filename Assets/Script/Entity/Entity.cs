using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }

    protected StateMachine stateMachine;
    
    protected bool facingRight = true;
    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInChildren<Rigidbody2D>();
    }
    

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        // 只有猴子需要状态机
        if (stateMachine != null)
            stateMachine.UpdateActiveState();
    }

    public void SetVelocity(float xVel, float yVel)
    {
        rb.linearVelocity = new Vector2(xVel, yVel);
        HandleFlip(xVel);
    }

    private void HandleFlip(float xVel)
    {
        if (xVel > 0 && facingRight == false) Flip();
        else if (xVel < 0 && facingRight) Flip();
    }
    
    private void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }
}
