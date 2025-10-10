using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using MonkeyPlayground.Components;
using MonkeyPlayground.Models;
using MonkeyPlayground.Models.ActionModel;
using UnityEngine;

namespace MonkeyPlayground.Objects
{
    public class Monkey : PerceptibleObject<MonkeyData>
    {
        private MoveableFeature _movement;
        private Collider2D _collider;
        private Vector2 _latestPosition;
        private Vector2 _latestSize;
        private ItemData? _holdingItemData;

        [SerializeField] public string monkeyName = "Monkey";
        [SerializeField] public Vector2 respawnPosition = Vector2.zero;
        [SerializeField] public float respawnHeight = -10.0f;

        [CanBeNull] public GraspableFeature HoldingItem { get; private set; }
        
        /// <summary>
        /// Indicate whether this monkey has reached the banana.
        /// </summary>
        public bool HasReachedBanana { get; internal set; }
        
        private void Start()
        {
            HasReachedBanana = false;
            _movement = GetComponent<MoveableFeature>();
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            _latestPosition = new Vector2
            {
                x = _collider.bounds.center.x,
                y = _collider.bounds.center.y - _collider.bounds.extents.y,
            };
            _latestSize = new Vector2
            {
                x = _collider.bounds.size.x,
                y = _collider.bounds.size.y,
            };
            if (transform.position.y < respawnHeight)
            {
                transform.position = new Vector3()
                {
                    x = respawnPosition.x,
                    y = respawnPosition.y,
                    z = transform.position.z
                };
                _movement.CancelMovement(
                    ActionResult.Failed(
                        "Monkey was teleported to the respawn point after falling off the platform."));
            }
        }
        
        protected override MonkeyData OnGenerateData()
        {
            return new MonkeyData
            {
                Name = monkeyName,
                Position = new PositionData
                {
                    X = _latestPosition.x,
                    Y = _latestPosition.y
                },
                Size = new SizeData
                {
                    Width = _latestSize.x,
                    Height = _latestSize.y
                },
                HoldingItem = _holdingItemData
            };
        }

        public void Move(float destination, [MaybeNull] Action<ActionResult> callback)
        {
            _movement.MoveAbsolutely(destination, callback);
        }

        public void ClimbUp([MaybeNull] Action<ActionResult> callback)
        {
            var target = ClimbableFeature.FindHighestClimbable(_collider);
            if (!target)
            {
                callback?.Invoke(ActionResult.Failed(
                    "Cannot find a suitable object for the monkey to climb. Possible reasons:" +
                    "(1) No climbable object within reach (x-axis distance <= 1.0 and on the same floor)." +
                    "(2) The nearby climbable object is too high to climb (height difference > 1.0)."));
                return;
            }

            var result = target.ClimbUp(_collider);
            callback?.Invoke(result);
        }

        public void ClimbDown([MaybeNull] Action<ActionResult> callback)
        {
            var counts = ClimbableFeature
                .FindStandingClimbable(_collider)
                .Count(climbable => climbable.ClimbDown(_collider));
            if (counts == 0)
                callback?.Invoke(ActionResult.Failed(
                    "Monkey is not standing on a box or a floor that it can jump down."));
            else
                callback?.Invoke(
                    ActionResult.Succeeded(
                        "Monkey successfully jumped down from the box or the floor that it was standing on."));
        }

        public void GrabItem([MaybeNull] Action<ActionResult> callback)
        {
            if (HoldingItem)
            {
                callback?.Invoke(ActionResult.Failed("This monkey is already holding an item." +
                                                     "It cannot grab another one until it drops the current item."));
                return;
            }

            var target = GraspableFeature.FindNearestGraspableObject(_collider);
            if (!target)
            {
                callback?.Invoke(
                    ActionResult.Failed(
                        "No graspable object within reach (x-axis distance <= 1.0 and on the same floor)."));
                return;
            }

            target.Grab(_collider);
            HoldingItem = target;
            _holdingItemData = target.GetComponent<Item>().GenerateData();
            callback?.Invoke(ActionResult.Succeeded("Successfully grabbed the nearest item."));
        }

        public void DropItem([MaybeNull] Action<ActionResult> callback)
        {
            if (!HoldingItem)
            {
                callback?.Invoke(ActionResult.Failed("No item is currently being held by this monkey."));
                return;
            }

            HoldingItem.Drop();
            HoldingItem = null;
            _holdingItemData = null;
            callback?.Invoke(ActionResult.Succeeded("Successfully dropped the item."));
        }
    }
}