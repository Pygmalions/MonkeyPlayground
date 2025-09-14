using System;
using MonkeyPlayground.Data;
using MonkeyPlayground.Objects;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [RequireComponent(typeof(Item), typeof(Collider2D)),
     DisallowMultipleComponent]
    public class ClimbableFeature : MonoBehaviour
    {
        private Collider2D _collider;
        
        private void Awake()
        {
            Reset();
        }

        private void Reset()
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
        
        /// <summary>
        /// 带有高度检查的攀爬逻辑
        /// </summary>
        /// <param name="climberTransform">攀爬者的Transform</param>
        /// <returns>返回一个ActionResult，表示成功或失败</returns>
        public ActionResult Climb(Transform climberTransform)
        {
            // var featureCollider = GetComponent<Collider2D>();
            var climberCollider = climberTransform.GetComponent<Collider2D>();

            if (_collider == null || climberCollider == null)
            {
                return ActionResult.Failed("Missing collision body, unable to calculate climbing.");
            }

            var monkeyFeetY = climberCollider.bounds.min.y;
            var featureTopY = _collider.bounds.max.y;
            var heightDifference = featureTopY - monkeyFeetY;

            if (heightDifference > 1f || heightDifference < 0)
            {
                return ActionResult.Failed("It’s too far to the top to climb up!");
            }

            var climberHeight = climberCollider.bounds.size.y;
            climberTransform.position = new Vector3(
                transform.position.x,
                featureTopY + climberHeight * 0.5f,
                climberTransform.position.z
            );

            return ActionResult.Succeeded("Climbed successfully!");
        }
    }
}