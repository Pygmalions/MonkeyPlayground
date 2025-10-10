using MonkeyPlayground.Components;
using MonkeyPlayground.Objects;
using UnityEngine;

namespace MonkeyPlayground.Controllers
{
    [DisallowMultipleComponent, 
     RequireComponent(typeof(Monkey), typeof(MoveableFeature), typeof(Collider2D))]
    public class MonkeyKeyboardController : MonoBehaviour
    {
        private MoveableFeature _movement;
        private Collider2D _collider;
        private Monkey _monkey;
        
        private void Start()
        {
            _movement = GetComponent<MoveableFeature>();
            _collider = GetComponent<Collider2D>();
            _monkey = GetComponent<Monkey>();
        }
    
        private void Update()
        {
            if (!_movement)
                return;
            
            // Add key pressing check so that it won't interfere with REST API controller.
            var movement = 0.0f;
            if (Input.GetKey(KeyCode.A))
                movement -= 1.0f;
            if (Input.GetKey(KeyCode.D))
                movement += 1.0f;
            if (movement != 0.0)
                _monkey.Move(_monkey.transform.position.x + movement * 0.2f, null);
            if (Input.GetKeyDown(KeyCode.W))
                _monkey.ClimbUp(null);
            if (Input.GetKeyDown(KeyCode.S))
                _monkey.ClimbDown(null);
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!_monkey.HoldingItem)
                    _monkey.GrabItem(null);
                else
                    _monkey.DropItem(null);
            }
        }
    }
}