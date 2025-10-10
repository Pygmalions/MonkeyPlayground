using UnityEngine;

namespace MonkeyPlayground.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(ClimbableFeature))]
    public class SquashableFeature : MonoBehaviour
    {
        private ClimbableFeature _climbable;

        [SerializeField, Range(0.0f, 1.0f)] 
        public float squashingPossibility = 0.5f;

        [SerializeField, Range(0.0f, 1.0f)] 
        public float squashingMaxFactor = 0.8f;
        
        [SerializeField, Range(0.0f, 1.0f)]
        public float squashingMinFactor = 0.5f;
        
        public bool IsSquashed { get; private set; }
        
        private void Start()
        {
            _climbable = GetComponent<ClimbableFeature>();
            _climbable.OnClimbed += _ => OnClimbed();
        }

        private void OnClimbed()
        {
            if (IsSquashed)
                return;
            if (Random.value < squashingPossibility)
                return;
            IsSquashed = true;
            transform.localScale = transform.localScale with
            {
                y = Random.Range(squashingMinFactor, squashingMaxFactor)
            };
        }
    }
}

