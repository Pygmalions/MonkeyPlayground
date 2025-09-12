using MonkeyPlayground.Components;
using MonkeyPlayground.Objects;
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

        public float velocity = 0.2f;
    
        private void Update()
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                monkeyMovement.MoveRelatively(-velocity, null);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                monkeyMovement.MoveRelatively(velocity, null);
            }
            else
            {
                // Currently there is no method to stop the target.
            }
        
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Todo: Make the monkey climb.
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (monkey.holdingItem == null)
                {
                    // Todo: Make the monkey pick up the nearest item.
                }
                else
                {
                    // Todo: Make the monkey drop the current item.
                }
            }
        }
    }
}

