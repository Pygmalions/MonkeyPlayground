using MonkeyPlayground.Components;
using MonkeyPlayground.Objects;
using MonkeyPlayground.Data.Actions;
using UnityEngine;

namespace MonkeyPlayground
{
    [DisallowMultipleComponent]
    public class MonkeyKeyboardController : MonoBehaviour
    {
        public Monkey monkey;
    
        /// <summary>
        /// Monkey for this keyboard controller to control.
        /// </summary>
        public ObjectMovementController monkeyMovement;
        
        private Monkey _monkey;
        private ObjectMovementController _movementController;
        
        private void Awake()
        {
            _monkey = GetComponent<Monkey>();
            _movementController = GetComponent<ObjectMovementController>();
        }
    
        private void Update()
        {
            // if (_monkey.ongoingAction != null) 
            //     return;

            var horizontal = Input.GetAxis("Horizontal");
            _movementController.MoveRelatively(horizontal, null);

            if (Input.GetKeyDown(KeyCode.E))
            {
                _monkey.AssignAction(new MonkeyClimbAction { Id = 0 });
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (_monkey.holdingBox == null)
                {
                    _monkey.AssignAction(new MonkeyGrabAction { Id = 0 });
                }
                else
                {
                    _monkey.AssignAction(new MonkeyDropAction { Id = 0 });
                }
            }
        }
    }
}

