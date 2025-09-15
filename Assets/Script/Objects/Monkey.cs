using System;
using System.Diagnostics.CodeAnalysis;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using UnityEngine;
using MonkeyPlayground.Components;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;

namespace MonkeyPlayground.Objects
{
    [DisallowMultipleComponent,
     RequireComponent(typeof(ObjectMovementController), typeof(Collider2D))]
    public class Monkey : PerceptibleObject<MonkeyData>
    {
        public string monkeyName = "Monkey";

        [Header("Interaction Settings")] [SerializeField]
        private Vector2 interactionRange = new(1.5f, 0.5f);

        /// <summary>
        /// The current ongoing action.
        /// </summary>
        [MaybeNull, ReadOnly] public ActionData ongoingAction;

        /// <summary>
        /// The current holding item.
        /// </summary>
        [MaybeNull, ReadOnly] public Item holdingItem;

        private ObjectMovementController _movementController;
        private Collider2D _monkeyCollider;
        private PositionData _latestPosition;
        private SizeData _latestSize;

        private void Awake()
        {
            _monkeyCollider = GetComponent<BoxCollider2D>();
            _movementController = GetComponent<ObjectMovementController>();
        }

        private void Update()
        {
            /*
             * Web threads cannot access Unity objects directly,
             * therefore status need to be cached here.
             */
            _latestPosition = new PositionData
            {
                X = (int)MathF.Round(transform.position.x + 0.5f),
                Y = (int)MathF.Round(transform.position.y),
            };
            _latestSize = new SizeData(transform);

            if (ongoingAction == null)
                return;

            if (ongoingAction.IsCompleted())
            {
                ongoingAction = null;
                return;
            }

            if (ongoingAction.Result.Status == ActionStatus.Pending)
                ExecuteAction(ongoingAction);

            if (ongoingAction.IsCompleted())
            {
                ongoingAction = null;
            }
        }

        protected override MonkeyData OnGenerateData()
        {
            return new MonkeyData
            {
                Name = monkeyName,
                Position = _latestPosition,
                Size = _latestSize,
                HoldingItem = holdingItem?.GenerateData(),
                OngoingAction = ongoingAction
            };
        }

        private void ExecuteAction(ActionData action)
        {
            ongoingAction = action;
            action.Result = ActionResult.Running();

            switch (ongoingAction)
            {
                case MonkeyMoveAction moving:
                    _movementController.MoveAbsolutely(moving.GoalPosition,
                        result => moving.Result = result);
                    break;
                case MonkeyGrabAction grab:
                    GrabItem(grab);
                    break;
                case MonkeyDropAction drop:
                    DropItem(drop);
                    break;
                case MonkeyClimbAction climb:
                    ClimbHighestBox(climb);
                    break;

                default:
                    ongoingAction.Result = ActionResult.Failed($"Unsupported action type '{ongoingAction.GetType()}'.");
                    break;
            }
        }

        /// <summary>
        /// Try to find the nearest interactable item on the same height level.
        /// </summary>
        /// <returns>Interactable item, or null if not found.</returns>
        public GraspableFeature FindNearestGraspableObject()
        {
            var center = _monkeyCollider.bounds.center +
                         Vector3.up * _monkeyCollider.bounds.size.y / 2;
            var hitColliders = Physics2D.OverlapBoxAll(
                center, interactionRange, 0f);
            if (hitColliders.Length == 0)
            {
                return null;
            }

            var nearestBox = hitColliders
                // Box must be graspable and not currently held by any monkey.
                .Select(hitCollider => hitCollider.GetComponent<GraspableFeature>())
                .NotNull()
                .Where(graspable => !graspable.IsPickedUp)
                .OrderBy(item => MathF.Abs(transform.position.x - item.transform.position.x))
                .FirstOrDefault();
            return nearestBox;
        }

        /// <summary>
        /// Try to find the highest climbable box on the same height level.
        /// </summary>
        /// <returns>Climbable box, or null if not found.</returns>
        public ClimbableFeature FindHighestClimbableObject()
        {
            var interactionSize = interactionRange;
            var monkeyCollider = GetComponent<Collider2D>();
            if (monkeyCollider == null) 
                return null;
            
            var hitColliders = Physics2D.OverlapBoxAll(
                monkeyCollider.bounds.center, interactionSize, 0f);

            return hitColliders
                .Select(hitCollider => hitCollider.GetComponent<ClimbableFeature>())
                .NotNull()
                .Where(climbable => climbable.IsSelectable(monkeyCollider))
                // Monkey should not climb the object which is currently being held.
                .Where(climbable => climbable.gameObject.GetComponent<GraspableFeature>()
                    is not { IsPickedUp: true })
                .OrderByDescending(climbable =>
                    climbable.GetComponent<Collider2D>().bounds.max.y)
                .FirstOrDefault();
        }

        public void ClimbHighestBox(MonkeyClimbAction climb)
        {
            var featureToClimb = FindHighestClimbableObject();
            if (featureToClimb != null)
                climb.Result = featureToClimb.Climb(transform, _monkeyCollider);
            else
                climb.Result = ActionResult.Failed("There are no climbable boxes or floors nearby.");
        }

        public void GrabItem(MonkeyGrabAction grab)
        {
            var graspable = FindNearestGraspableObject();
            if (graspable != null)
            {
                var itemCollider = graspable.GetComponent<Collider2D>();
                var floatingPositionY = 
                    (_monkeyCollider.bounds.size.y + itemCollider.bounds.size.y) / 2;
                graspable.OnPickup();

                if (graspable.GetComponent<Item>() is not { } graspableItem)
                    throw new Exception("Graspable object is not an Item.");
                holdingItem = graspableItem;
                graspable.transform.SetParent(transform);
                
                graspable.transform.localPosition = new Vector3(0, floatingPositionY, 0);
                grab.Result = ActionResult.Succeeded("Successfully picked up the item.");
            }
            else
            {
                grab.Result = ActionResult.Failed("There are no items nearby to pick up.");
            }
        }


        public void DropItem(MonkeyDropAction drop)
        {
            if (holdingItem != null)
            {
                if (holdingItem.GetComponent<GraspableFeature>() is not { } graspable)
                    throw new Exception("Holding item does not have a GraspableFeature component.");
                holdingItem.transform.SetParent(null);
                graspable.OnDrop();
                holdingItem.transform.position = holdingItem.transform.position with
                {
                    y = _monkeyCollider.bounds.center.y + 
                        holdingItem.transform.localScale.y / 2
                };

                holdingItem = null;
                drop.Result = ActionResult.Succeeded("The held item has been successfully dropped.");
            }
            else
            {
                drop.Result = ActionResult.Failed("No item is currently held by the monkey.");
            }
        }
    }
}