using MonkeyPlayground.Data;
using MonkeyPlayground.Objects;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [RequireComponent(typeof(Item), typeof(Collider2D))]
    public class ClimbableFeature : MonoBehaviour
    {
        /// <summary>
        /// 带有高度检查的攀爬逻辑
        /// </summary>
        /// <param name="climberTransform">攀爬者的Transform</param>
        /// <returns>返回一个ActionResult，表示成功或失败</returns>
        public ActionResult AttemptClimb(Transform climberTransform)
        {
            var featureCollider = GetComponent<Collider2D>();
            var climberCollider = climberTransform.GetComponent<Collider2D>();

            if (featureCollider == null || climberCollider == null)
            {
                return ActionResult.Failed("Missing collision body, unable to calculate climbing.");
            }

            float monkeyFeetY = climberCollider.bounds.min.y;
            float featureTopY = featureCollider.bounds.max.y;
            float heightDifference = featureTopY - monkeyFeetY;

            if (heightDifference > 1f || heightDifference < 0)
            {
                return ActionResult.Failed($"It’s too far to the top to climb up!");
            }
            
            float climberHeight = climberCollider.bounds.size.y;
            climberTransform.position = new Vector3(
                transform.position.x,
                featureTopY + climberHeight * 0.5f,
                climberTransform.position.z
            );
            
            return ActionResult.Succeeded("Climbed successfully!");
        }
    }
}