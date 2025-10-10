using System;
using System.Collections;
using JetBrains.Annotations;
using MonkeyPlayground.Models.ActionModel;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    public record struct MovementCommand(float TargetX, [CanBeNull] Action<ActionResult> Callback);

    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class MoveableFeature : MonoBehaviour
    {
        private static readonly int Velocity = Animator.StringToHash("velocity");

        public bool IsMoving { get; private set; }

        [SerializeField] public float velocity = 3.0f;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;

        private MovementCommand? _command;
        private LayerMask _floorLayer;

        private void Start()
        {
            _floorLayer = LayerMask.GetMask("Terrain", "Box");
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (!IsMoving)
                _rigidbody.linearVelocity = _rigidbody.linearVelocity with { x = 0.0f };
        }

        public void MoveRelatively(float offset, Action<ActionResult> onComplete)
        {
            MoveAbsolutely(transform.position.x + offset, onComplete);
        }

        public void MoveAbsolutely(float targetX, [CanBeNull] Action<ActionResult> callback)
        {
            if (IsMoving)
            {
                _command?.Callback?.Invoke(ActionResult.Cancelled("This action is overriden with a new move command."));
                return;
            }

            _command = new MovementCommand(targetX, callback);
            StartCoroutine(ChaseTargetPositionCoroutine());
        }

        public void CancelMovement(ActionResult result)
        {
            _command?.Callback?.Invoke(result);
            _command = null;
        }

        private IEnumerator ChaseTargetPositionCoroutine()
        {
            if (_command == null)
                yield break;

            if (!IsMoving)
                EnterMovingState();

            while (_command != null && Mathf.Abs(_command.Value.TargetX - transform.position.x) > 0.05f)
            {
                var targetX = _command.Value.TargetX;

                // If the object is not on the ground, pause moving.
                if (!Physics2D.OverlapBox(transform.position,
                        new Vector2(_collider.bounds.size.x, _collider.bounds.size.y),
                        0, _floorLayer))
                {
                    SetVelocity(0, _rigidbody.linearVelocity.y);
                    yield return null;
                    continue;
                }

                var direction = Mathf.Sign(targetX - transform.position.x);

                SetVelocity(velocity * direction, _rigidbody.linearVelocity.y);

                yield return null;
            }

            SetVelocity(0, _rigidbody.linearVelocity.y);

            if (IsMoving)
                ExitMovingState(ActionResult.Succeeded("Monkey has reached the target position."));

            yield break;

            void SetVelocity(float xVelocity, float yVelocity)
            {
                _rigidbody.linearVelocity = new Vector2(xVelocity, yVelocity);
                if (_animator)
                    _animator.SetFloat(Velocity, Math.Abs(xVelocity));
                if ((xVelocity > 0.05f && transform.rotation.y <= -0.5f) ||
                    (xVelocity < -0.05f && transform.rotation.y >= -0.5f))
                    transform.Rotate(0, 180, 0);
            }

            void EnterMovingState()
            {
                IsMoving = true;
            }

            void ExitMovingState(ActionResult result)
            {
                IsMoving = false;
                _command?.Callback?.Invoke(result);
            }
        }
    }
}