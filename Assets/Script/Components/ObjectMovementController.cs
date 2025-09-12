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
            MoveAbsolutely(transform.position.x + offset, onComplete);
        }

        public void MoveAbsolutely(float targetPosition, Action<ActionResult> onComplete)
        {
            if (IsMoving)
            {
                _targetPosition = targetPosition;
                _continuation?.Invoke(ActionResult.Cancelled("This action is overriden with a new move command."));
                _continuation = onComplete;
                return;
            }

            _targetPosition = targetPosition;
            _continuation = onComplete;
            StartCoroutine(ChaseTargetPositionCoroutine());
        }

        public IEnumerator ChaseTargetPositionCoroutine()
        {
            if (!IsMoving)
                EnterMovingState();

            while (Mathf.Abs(_targetPosition - transform.position.x) > 0.05f)
            {
                var raycastDistance = _collider.bounds.size.x * 0.5f + 0.1f;
                var direction = Mathf.Sign(_targetPosition - transform.position.x);
                var rayOrigin = new Vector2(transform.position.x, _collider.bounds.center.y);
                var rayDirection = new Vector2(direction, 0);
                Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.red);

                var hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance, obstacleLayer);
                if (hit.collider != null)
                {
                    Debug.Log($"Obstacle detected: {hit.collider.name}");
                    SetVelocity(0, _rigidBody.linearVelocity.y);

                    ExitMovingState(
                        ActionResult.Failed(
                            "Failed to move to the specified position: path is blocked by obstacle."));

                    yield break;
                }

                SetVelocity(velocity * direction, _rigidBody.linearVelocity.y);

                if ((direction > 0 && transform.position.x > _targetPosition) ||
                    (direction < 0 && transform.position.x < _targetPosition))
                {
                    break;
                }

                yield return null;
            }

            transform.position = new Vector3(_targetPosition, transform.position.y, transform.position.z);

            ExitMovingState(ActionResult.Succeeded("Monkey has reached the target position."));

            yield break;

            void SetVelocity(float xVelocity, float yVelocity)
            {
                _rigidBody.linearVelocity = new Vector2(xVelocity, yVelocity);
                if ((xVelocity > 0 && _isFacingRight) || (xVelocity < 0 && !_isFacingRight))
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