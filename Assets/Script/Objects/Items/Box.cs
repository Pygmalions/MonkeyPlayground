using UnityEngine;
using MonkeyPlayground.Data;
using UnityEngine.Serialization;

namespace MonkeyPlayground.Objects.Items
{

    public class Box : Item
    {
        public Box()
        {
            itemName = "Box";
        }

        public bool isClimbable = true; //万一有不能爬的箱子呢

        // 恢复之前的猴子可以左右穿过同时可以踩的逻辑
        private Collider2D _monkeyCollider;
        private Collider2D _boxSolidCollider;
        private Rigidbody2D _boxrigidbody;
        private RigidbodyType2D _originalBodyType;
        private const float PlatformTopBuffer = 0.05f;

        private void Awake()
        {
            _boxSolidCollider = GetComponent<Collider2D>();
            _boxrigidbody = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// 在Update中持续检查猴子与箱子的相对位置
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (_monkeyCollider == null)
            {
                return;
            }

            float monkeyFeetY = _monkeyCollider.bounds.min.y;
            float boxTopY = _boxSolidCollider.bounds.max.y;

            if (monkeyFeetY < boxTopY - PlatformTopBuffer)
            {
                Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, true);
            }
            else
            {
                Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, false);
            }
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _monkeyCollider = other;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (_monkeyCollider != null)
                {
                    Physics2D.IgnoreCollision(_boxSolidCollider, _monkeyCollider, false);
                }

                _monkeyCollider = null;
            }
        }

        
        /// <summary>
        /// 【新增】当物品被拿起时调用
        /// </summary>
        public void OnPickup()
        {
            _originalBodyType = _boxrigidbody.bodyType;
            _boxrigidbody.bodyType = RigidbodyType2D.Kinematic;
            _boxrigidbody.linearVelocity = Vector2.zero; // 清空速度
            _boxSolidCollider.enabled = false;
        }

        /// <summary>
        /// 【新增】当物品被放下时调用
        /// </summary>
        public void OnDrop()
        {
            _boxrigidbody.bodyType = _originalBodyType;
            _boxSolidCollider.enabled = true;
        }

        /// <summary>
        /// 带有高度检查的攀爬逻辑
        /// </summary>
        /// <param name="climberTransform">攀爬者的Transform</param>
        /// <returns>返回一个ActionResult，表示成功或失败</returns>
        public ActionResult AttemptClimb(Transform climberTransform)
        {
            var boxCollider = GetComponent<Collider2D>();
            var climberCollider = climberTransform.GetComponent<Collider2D>();

            if (boxCollider == null || climberCollider == null)
            {
                return ActionResult.Failed("Missing collision body, unable to calculate climbing.");
            }


            float monkeyFeetY = climberCollider.bounds.min.y;
            float boxTopY = boxCollider.bounds.max.y;
            float heightDifference = boxTopY - monkeyFeetY;

            // 高度差在0到1f之间才允许攀爬，否则失败
            if (heightDifference > 1f || heightDifference < 0)
            {
                return ActionResult.Failed($"It’s too far from the top of the box and I can’t climb up!");
            }

            // 执行攀爬动作
            float climberHeight = climberCollider.bounds.size.y;
            climberTransform.position = new Vector3(
                transform.position.x,
                boxTopY + 0.05f,
                climberTransform.position.z
            );

            return ActionResult.Succeeded("Successfully climbed onto the box!");
        }
    }
}