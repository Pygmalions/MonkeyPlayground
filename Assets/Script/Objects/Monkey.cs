using System.Diagnostics.CodeAnalysis;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent]
    public class Monkey : PerceptibleObject<MonkeyData>
    {
        /// <summary>
        /// Cached latest position of the monkey.
        /// </summary>
        private PositionData _latestPosition;

        /// <summary>
        /// Cached latest size of the monkey.
        /// </summary>
        private SizeData _latestSize;

        [SerializeField] public string monkeyName = "Monkey";

        /// <summary>
        /// The current ongoing action.
        /// </summary>
        [MaybeNull] public ActionData ongoingAction;

        /// <summary>
        /// The current holding item.
        /// </summary>
        [MaybeNull] public Item holdingItem;

        protected override MonkeyData OnGenerateData()
        {
            return new MonkeyData
            {
                Name = monkeyName,
                Position = _latestPosition,
                Size = _latestSize,
                HoldingItem = holdingItem.GenerateData(),
                OngoingAction = ongoingAction
            };
        }
        
        /// <summary>
        /// Assign an action to this monkey.
        /// </summary>
        /// <param name="action">Action to assign to this monkey.</param>
        /// <returns>Action data.</returns>
        public ActionData AssignAction(ActionData action)
        {
            ongoingAction = action;
            return ongoingAction;
        }

        /// <summary>
        /// Try to find the nearest interactable item on the same height level.
        /// </summary>
        /// <returns>Interactable item, or null if not found.</returns>
        public Item FindInteractableItem()
        {
            // Todo: Find the nearest interactable item.
            return null;
        }

        private void Update()
        {
            /*
             * Web threads cannot access Unity objects directly,
             * therefore status need to be cached here.
             */
            _latestPosition = new PositionData(transform);
            _latestSize = new SizeData(transform);

            if (ongoingAction == null)
                return;

            switch (ongoingAction)
            {
                case MonkeyMovingAction moving:
                {
                    // Todo: Move monkey to the goal position.
                    break;
                }
                // Todo: Support grabbing and dropping items.
                // Todo: Support climbing boxes.
                default:
                    Debug.LogError($"Unsupported action of type '{ongoingAction.GetType()}'.");
                    break;
            }
        }
    }
}