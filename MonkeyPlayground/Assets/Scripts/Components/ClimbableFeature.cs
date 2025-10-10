using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonkeyPlayground.Models.ActionModel;
using Unity.VisualScripting;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class ClimbableFeature : MonoBehaviour
    {
        private EdgeCollider2D _collider;

        private PlatformEffector2D _effector;
        
        private void Awake()
        {
            _collider = GetComponent<EdgeCollider2D>();
            _effector = GetComponent<PlatformEffector2D>();
        }

        public bool enableFallThrough = true;

        public static ClimbableFeature FindHighestClimbable(Collider2D climberCollider)
        {
            var climberTransform = climberCollider.transform;
            
            var regionPosition = new Vector2()
            {
                x = climberTransform.position.x,
                y = climberTransform.position.y + climberCollider.bounds.extents.y
            };
            
            var regionSize = new Vector2()
            {
                x = climberCollider.bounds.size.x + 1.0f,
                y = 2.1f
            };
            
            return Physics2D.OverlapBoxAll(regionPosition, regionSize, 0)
                .Select(target => (Collider: target, Feature: target.GetComponent<ClimbableFeature>()))
                .Where(target => target.Feature)
                .Where(target => target.Feature.GetComponent<GraspableFeature>()?.IsPickedUp != true)
                .Where(target => target.Collider.bounds.max.y > climberCollider.bounds.min.y)
                .Where(target => target.Collider.bounds.max.y - climberCollider.bounds.max.y < 1.05f)
                .OrderByDescending(target => target.Collider.bounds.max.y)
                .Select(target => target.Feature)
                .FirstOrDefault();
        }

        public event Action<Collider2D> OnClimbed;
        
        public ActionResult ClimbUp(Collider2D climberCollider)
        {
            // Disable collision ignore.
            Physics2D.IgnoreCollision(climberCollider, _collider, false);
            
            var climberTransform = climberCollider.transform;
            
            var destinationX = climberTransform.position.x;
            destinationX = Mathf.Max(destinationX, _collider.bounds.min.x + 0.1f);
            destinationX = Mathf.Min(destinationX, _collider.bounds.max.x - 0.1f);

            climberTransform.position = climberTransform.position with
            {
                x = destinationX,
                y = _collider.bounds.max.y + 0.05f
            };
            
            OnClimbed?.Invoke(climberCollider);
            
            return ActionResult.Succeeded("Successfully climbed onto the highest object within reach.");
        }
        
        public static IEnumerable<ClimbableFeature> FindStandingClimbable(Collider2D climberCollider)
        {
            return Physics2D.RaycastAll(climberCollider.bounds.center, Vector2.down,
                    climberCollider.bounds.extents.y + 0.1f)
                .OrderBy(hit => hit.collider.bounds.min.y)
                .Select(hit => hit.collider.GetComponent<ClimbableFeature>())
                .NotUnityNull();
        }

        public bool ClimbDown(Collider2D climberCollider)
        {
            if (!enableFallThrough)
                return false;
            StartCoroutine(FallCoroutine(climberCollider));
            return true;
        }
        
        private IEnumerator FallCoroutine(Collider2D climberCollider)
        {
            Physics2D.IgnoreCollision(_collider, climberCollider, true);

            var time = 0.0f;

            while (time < 0.5f && climberCollider.bounds.max.y - _collider.bounds.max.y > _collider.bounds.extents.y)
            {
                time += Time.deltaTime;
                yield return null;
            }
            
            Physics2D.IgnoreCollision(_collider, climberCollider, false);
        }
    }
}

