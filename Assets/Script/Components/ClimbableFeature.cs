using System;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
    public class ClimbableFeature : MonoBehaviour
    {
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        /// <summary>
        /// Check whether the climber can select this climbable object to climb.
        /// The top of this object must not be lower than the feet of the climber,
        /// and the height difference must not exceed 1 unit.
        /// </summary>
        /// <param name="climberCollider">Collider of the climber.</param>
        /// <returns>True if the climber can reach this object.</returns>
        public bool IsSelectable(Collider2D climberCollider)
        {
            var monkeyFeetY = climberCollider.bounds.min.y;
            var featureTopY = _collider.bounds.max.y;
            var heightDifference = featureTopY - monkeyFeetY;

            return heightDifference is <= 1.05f and > 0;
        }
        
        public ActionResult Climb(Transform climberTransform, Collider2D climberCollider)
        {
            var climberBottomY = climberCollider.bounds.min.y;
            var destinationY = _collider.bounds.max.y;
            var heightDifference = destinationY - climberBottomY;
            if (heightDifference is > 1f or < 0)
            {
                return ActionResult.Failed(
                    "Failed to climb the object: it is either too heigh or below the monkey.");
            }
            
            var destinationX = climberTransform.position.x;
            destinationX = MathF.Max(destinationX, _collider.bounds.min.x + 0.1f);
            destinationX = MathF.Min(destinationX, _collider.bounds.max.x - 0.1f);
            
            climberTransform.position = climberTransform.position with
            {
                x = destinationX,
                y = destinationY
            };

            return ActionResult.Succeeded("Climbed successfully!");
        }
    }
}