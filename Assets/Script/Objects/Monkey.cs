using System.Diagnostics.CodeAnalysis;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using UnityEngine;
using MonkeyPlayground.Components;
using MonkeyPlayground.Objects.Items;
using System.Linq;
using UnityEngine.Serialization;

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

        [Header("Interaction Settings")] 
        [SerializeField] private float interactionwidth = 1.5f;
        [SerializeField] private float interactionheight = 0.5f;
        [SerializeField] private LayerMask obstacleLayer;

        /// <summary>
        /// The current ongoing action.
        /// </summary>
        [MaybeNull] public ActionData ongoingAction;

        /// <summary>
        /// The current holding item.
        /// </summary>
        [FormerlySerializedAs("holdingItem")] [MaybeNull] public Box holdingBox;
        
        private ObjectMovementController _movementController;

        protected override MonkeyData OnGenerateData()
        {
            return new MonkeyData
            {
                Name = monkeyName,
                Position = _latestPosition,
                Size = _latestSize,
                HoldingItem = holdingBox?.GenerateData(),
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
        public Box FindInteractableBox()
        {
            var monkeyCollider = GetComponent<Collider2D>();
            if (monkeyCollider == null)
            {
                Debug.LogError("Monkey is missing a Collider2D component.");
                return null;
            }
    
            Vector2 center = monkeyCollider.bounds.center + Vector3.up * monkeyCollider.bounds.size.y / 2;
            Vector2 interactionSize = new Vector2(interactionwidth, interactionheight);
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(center, interactionSize, 0f);

            if (hitColliders.Length == 0)
            {
                return null;
            }

            // 在小范围内进行精确查找
            float verticalTolerance = monkeyCollider.bounds.size.y * 0.5f;

            var nearestBox = hitColliders
                .Select(col => col.GetComponent<Box>())
                .Where(item => item != null)
                .Where(item => item != holdingBox)
                .OrderBy(item => Vector2.Distance(
                    new Vector2(transform.position.x, 0),
                    new Vector2(item.transform.position.x, 0)
                ))
                .FirstOrDefault();

            return nearestBox;
        }
        
        /// <summary>
        /// Try to find the highest climbable box on the same height level.
        /// </summary>
        /// <returns>Climbable box, or null if not found.</returns>
        public Box FindHighestClimbableBox()
        {
            var monkeyCollider = GetComponent<Collider2D>();
            if (monkeyCollider == null)
            {
                Debug.LogError("Monkey is missing a Collider2D component.");
                return null;
            }

            Vector2 center = monkeyCollider.bounds.center + Vector3.up * monkeyCollider.bounds.size.y / 2;
            Vector2 interactionSize = new Vector2(interactionwidth, interactionheight);
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(center, interactionSize, 0f);
            
            if (hitColliders.Length == 0)
            {
                return null; 
            }

            float monkeyFeetY = monkeyCollider.bounds.min.y;
            const float maxClimbHeight = 1f; 

            var highestBox = hitColliders
                .Select(col => col.GetComponent<Item>())
                .OfType<Box>()
                .Where(box => box.isClimbable)
                .Where(box =>
                {
                    var boxCollider = box.GetComponent<Collider2D>();
                    float boxTopY = boxCollider.bounds.max.y;
                    float heightDifference = boxTopY - monkeyFeetY;
                    return heightDifference >= 0 && heightDifference <= maxClimbHeight;
                })
                .OrderByDescending(box => box.GetComponent<Collider2D>().bounds.max.y)
                .FirstOrDefault();

            return highestBox;
        }
        
        private void Awake()
        {
            _movementController = GetComponent<ObjectMovementController>();
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

            if (ongoingAction.IsCompleted())
            {
                ongoingAction = null;
                return;
            }
            
            if (ongoingAction.Result.Status == ActionStatus.Pending)
                ExecuteAction(ongoingAction);
            
            if (ongoingAction.IsCompleted())
            {
                // 在同一帧内就将 ongoingAction 清空！
                ongoingAction = null;
            }
        }

        private void ExecuteAction(ActionData action)
        {
            ongoingAction = action;
            action.Result = ActionResult.Running();
            
            switch (ongoingAction)
            {
                case MonkeyMovingAction moving:
                {
                    _movementController.MoveAbsolutely(moving.GoalPosition, 
                        result => moving.Result = result);
                    break;
                }
                
                case MonkeyGrabAction grab:
                {
                    GrabNearestItem(grab);
                    break;
                }
                
                case MonkeyDropAction drop:
                {
                    DropItem(drop);
                    break;
                }
                
                case MonkeyClimbAction climb:
                {
                    ClimbHighestBox(climb);
                    break;
                }
                
                default:
                    ongoingAction.Result = ActionResult.Failed($"Unsupported action type '{ongoingAction.GetType()}'.");
                    break;
            }
        }
        
        public void ClimbHighestBox(MonkeyClimbAction climb)
        {
            var itemToClimb = FindHighestClimbableBox();
            if (itemToClimb != null && itemToClimb is Box climbableBox && climbableBox.isClimbable)
            {
                climb.Result = climbableBox.AttemptClimb(transform);
            }
            else
            {
                climb.Result = ActionResult.Failed("There are no climbable boxes nearby。");
            }
        }

        public void GrabNearestItem(MonkeyGrabAction grab)
        {
            var boxToGrab = FindInteractableBox();
            if (boxToGrab != null)
            {
                float boxwidth = boxToGrab.GetComponent<Collider2D>().bounds.size.x;
                float boxHeight = boxToGrab.GetComponent<Collider2D>().bounds.size.y;
                boxToGrab.OnPickup(); // OnPickup()函数用于禁用箱子的碰撞体和动力学
        
                holdingBox = boxToGrab;
                boxToGrab.transform.SetParent(transform);
                
                int facingDirection = _movementController.IsFacingRight ? 1 : -1;
                Vector3 floatPosition = new Vector3(boxwidth/2, boxHeight, 0);
                boxToGrab.transform.localPosition = floatPosition;
        
                grab.Result = ActionResult.Succeeded("Successfully picked up the item!");
            }
            else
            {
                grab.Result = ActionResult.Failed("There are no items nearby to pick up.");
            }
        }

        
        public void DropItem(MonkeyDropAction drop)
        {
            if (holdingBox != null)
            {
                var itemCollider = holdingBox.GetComponent<Collider2D>();
        
                itemCollider.enabled = true;
                Collider2D overlapCheck = Physics2D.OverlapBox(holdingBox.transform.position, itemCollider.bounds.size * 0.9f, 0, obstacleLayer);
                
                if (overlapCheck != null)
                {
                    drop.Result = ActionResult.Failed("Placement failed: The target location is blocked by an obstacle!");
                }
                else
                {
                    Vector3 dropPosition = holdingBox.transform.position;
                    holdingBox.transform.SetParent(null);
                    holdingBox.transform.position = dropPosition;
                    holdingBox.OnDrop(); // OnDrop 会最终确保碰撞体是启用的
                    holdingBox = null;
                    drop.Result = ActionResult.Succeeded("Successfully dropped the item!");
                }
            }
            else
            {
                drop.Result = ActionResult.Failed("There are no items being held.");
            }
        }
        
    }
}