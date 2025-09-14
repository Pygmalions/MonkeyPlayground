using MonkeyPlayground.Data;
using MonkeyPlayground.Objects;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [RequireComponent(typeof(Item), typeof(SpriteRenderer)),
     DisallowMultipleComponent]
    public class InvisibleFeature : MonoBehaviour
    {
        [SerializeField] private float revealRadius = 2f;

        private Item _item;
        private SpriteRenderer _spriteRenderer;
        private Transform _monkeyTransform;

        private Color _originalColor;
        private bool _isObscured = true;

        private void Awake()
        {
            _item = GetComponent<Item>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            var monkey = Object.FindFirstObjectByType<Monkey>();
            if (monkey != null)
            {
                _monkeyTransform = monkey.transform;
            }
            else
            {
                Debug.LogError("No monkey found in the scene, ObscurableItem doesn't work!", this);
                enabled = false; //没有猴子这个脚本没必要工作吧
                return;
            }

            _originalColor = _spriteRenderer.color;

            _item.OnGeneratingData += ModifyItemData;
        }

        private void OnDestroy()
        {
            if (_item != null)
            {
                _item.OnGeneratingData -= ModifyItemData;
            }
        }

        private void Update()
        {
            if (_monkeyTransform == null) return;

            var distanceToMonkey = Vector2.Distance(transform.position, _monkeyTransform.position);

            if (distanceToMonkey <= revealRadius)
            {
                Reveal();
            }
            else
            {
                Obscure();
            }
        }

        private void Obscure()
        {
            _isObscured = true;
            _spriteRenderer.color = Color.gray;
        }

        private void Reveal()
        {
            _isObscured = false;
            _spriteRenderer.color = _originalColor;
        }

        /// <summary>
        /// Method that responds to the OnGeneratingData event
        /// </summary>
        private ItemData ModifyItemData(ItemData originalData)
        {
            if (_isObscured)
            {
                return originalData with
                {
                    Name = "Unknown",
                    Description = "An unknown object, you can only know what it is when you get closer"
                };
            }

            return originalData;
        }

        /// <summary>
        /// Draw a visual range circle in the editor to facilitate debugging
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, revealRadius);
        }
    }
}