using System.Linq;
using MonkeyPlayground.Models;
using MonkeyPlayground.Objects;
using UnityEngine;

namespace MonkeyPlayground.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(Item), typeof(SpriteRenderer))]
    public class InvisibleFeature : MonoBehaviour
    {
        [SerializeField] public float visibleDistance = 1.0f;

        public bool Visible { get; private set; }

        private Item _item;
        private LayerMask _layer;
        private SpriteRenderer _sprite;
        private Color? _originalColor;
        
        private void Start()
        {
            _layer = LayerMask.GetMask("Monkey");
            _item = GetComponent<Item>();
            _item.OnGeneratingData += OnGenerateItemData;
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Visible = Physics2D
                .OverlapCircleAll(transform.position, visibleDistance, _layer)
                .Any(target => target.GetComponent<Monkey>());
            if (!Visible)
            {
                _originalColor ??= _sprite.color;
                _sprite.color = Color.gray;
            }
            else if (_originalColor != null)
            {
                _sprite.color = _originalColor.Value;
            }
        }

        private ItemData OnGenerateItemData(ItemData data)
        {
            if (Visible)
                return data;
            
            return new ItemData()
            {
                Id = data.Id,
                Position = data.Position,
                Functions = null,
                Name = "Unknown",
                Description = "An unknown object, the monkey can't see it clearly unless it gets close enough. " +
                              "Note that the size of this object is -1 for both width and height, indicating that " +
                              "the monkey currently cannot know its actual size.",
                Size = new SizeData { Width = -1, Height = -1 },
            };
        }
    }
}