using System;
using System.Collections;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    public class ObjectMovementController : MonoBehaviour
    {
        public bool IsMoving { get; private set; }

        [SerializeField] public LayerMask obstacleLayer;

        [SerializeField] public float velocity = 1.0f;

        private bool _isFacingRight = true;
        
        public bool IsFacingRight => _isFacingRight;

        private Animator _animator;

        private Rigidbody2D _rigidBody;

        private Collider2D _collider;

        private float _targetPosition;

        private Action<ActionResult> _continuation;

        private void Start()
        {
            obstacleLayer = LayerMask.GetMask("Ground");
            _animator = GetComponent<Animator>();
            _rigidBody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        public void MoveRelatively(float offset, Action<ActionResult> onComplete)
        {
            // Compensate for the -0.5 in alignment with the center of the tile.
            MoveAbsolutely(transform.position.x + offset + 0.5f, onComplete);
        }

        public void MoveAbsolutely(float targetPosition, Action<ActionResult> onComplete)
        {
            // Minus 0.5 to align with the center of the tile.
            _targetPosition = targetPosition - 0.5f;
            
            if (IsMoving)
            {
                _continuation?.Invoke(ActionResult.Cancelled("This action is overriden with a new move command."));
                _continuation = onComplete;
                return;
            }
            
            _continuation = onComplete;
            StartCoroutine(ChaseTargetPositionCoroutine());
        }

        public IEnumerator ChaseTargetPositionCoroutine()
        {
            if (!IsMoving)
                EnterMovingState();

            while (Mathf.Abs(_targetPosition - transform.position.x) > 0.05f)
            {
                var raycastDistance = _collider.bounds.extents.x + 0.1f;
                var direction = Mathf.Sign(_targetPosition - transform.position.x);
                var rayOrigin = new Vector2(_collider.bounds.center.x, _collider.bounds.center.y);
                var rayDirection = new Vector2(direction, 0);
                Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red);

                var hit = Physics2D.Raycast(
                    rayOrigin, rayDirection, raycastDistance, obstacleLayer);
                if (hit.collider != null)
                    break;

                SetVelocity(velocity * direction, _rigidBody.linearVelocity.y);

                if ((direction > 0 && transform.position.x > _targetPosition) ||
                    (direction < 0 && transform.position.x < _targetPosition))
                {
                    break;
                }

                yield return null;
            }
            
            SetVelocity(0, _rigidBody.linearVelocity.y);

            if (Mathf.Abs(_targetPosition - transform.position.x) > 0.1f)
            {
                Debug.Log("Failed to reach the target position due to an obstacle.");
                ExitMovingState(
                    ActionResult.Failed(
                        "Failed to move to the specified position: path is blocked by obstacle."));
            }
            
            ExitMovingState(ActionResult.Succeeded("Monkey has reached the target position."));

            yield break;

            void SetVelocity(float xVelocity, float yVelocity)
            {
                _rigidBody.linearVelocity = new Vector2(xVelocity, yVelocity);
                if ((xVelocity >= 0 && _isFacingRight) || (xVelocity <= 0 && !_isFacingRight))
                    return;
                transform.Rotate(0, 180, 0);
                _isFacingRight = !_isFacingRight;
            }

            void EnterMovingState()
            {
                IsMoving = true;
                if (_animator == null)
                    return;
                _animator.SetBool("move", true);
                _animator.SetBool("idle", false);
            }

            void ExitMovingState(ActionResult result)
            {
                IsMoving = false;
                _continuation?.Invoke(result);
                _continuation = null;
                if (_animator == null)
                    return;
                _animator.SetBool("move", false);
                _animator.SetBool("idle", true);
            }
        }
    }
}