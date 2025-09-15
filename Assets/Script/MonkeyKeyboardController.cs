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
            // Add key pressing check so that it won't interfere with REST API controller.
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || 
                Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                _movementController.MoveRelatively(
                    Input.GetAxis("Horizontal"), null);
            }
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                _monkey.ongoingAction = new MonkeyClimbAction {Id = -1};
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (_monkey.holdingItem == null)
                    _monkey.ongoingAction = new MonkeyGrabAction {Id = -1};
                else
                    _monkey.ongoingAction = new MonkeyDropAction {Id = -1};
            }
        }
    }
}

