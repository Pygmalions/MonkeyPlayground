using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using MonkeyPlayground.Objects;
using UnityEngine;
using RestServer;
using UnityEngine.SceneManagement;

namespace MonkeyPlayground
{
    [RequireComponent(typeof(RestServer.RestServer)), DisallowMultipleComponent]
    public class MonkeyHttpController : MonoBehaviour
    {
        private static int _actionNextId = 0;

        public RestServer.RestServer server;

        /// <summary>
        /// Monkey controlled by this controller.
        /// There might be multiple monkeys in the scene,
        /// but only one is controlled by this controller.
        /// </summary>
        [Tooltip("The monkey controlled by this controller.")]
        public Monkey monkey;

        public string sceneName = "Normal Scene";

        [Multiline(int.MaxValue)] public string sceneDescription = "This is a classical monkey-banana problem.";

        public Item[] DiscoveredItems { get; private set; }

        public Floor[] DiscoveredFloors { get; private set; }

        // private readonly Dictionary<int, ActionData> _actions = new();

        private readonly MemoryCache _actions = new("ActionDataCache");

        /// <summary>
        /// This flag indicates whether the task is completed.
        /// It is set by the banana item.
        /// </summary>
        public bool IsTaskCompleted { get; internal set; }
        
        /// <summary>
        /// Scan the whole scene for items and floors.
        /// </summary>
        public void ScanPerceptibleObjects()
        {
            DiscoveredItems = FindObjectsByType<Item>(FindObjectsSortMode.None);
            DiscoveredFloors = FindObjectsByType<Floor>(FindObjectsSortMode.None);
        }

        private void Reset()
        {
            server = GetComponent<RestServer.RestServer>();
            monkey = GetComponent<Monkey>();
            IsTaskCompleted = false;
        }

        private float _actionWaitingTime = 0.0f;
        
        private void Start()
        {
            IsTaskCompleted = false;
            
            ScanPerceptibleObjects();

            _actionWaitingTime = Time.fixedDeltaTime * 2;

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/scene/status",
                request => { request.CreateResponse().BodyJson(GetSceneData()).SendAsync(); });

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/scene/description",
                request =>
                {
                    request.CreateResponse().BodyJson(new
                    {
                        Name = sceneName,
                        Description = sceneDescription
                    }).SendAsync();
                });

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/action/status",
                request =>
                {
                    var stringId = request.QueryParametersDict["id"].FirstOrDefault();
                    if (!int.TryParse(stringId, out var id))
                    {
                        request.CreateResponse().StatusError()
                            .Body("Action ID is not a valid integer.")
                            .SendAsync();
                        return;
                    }

                    var data = SearchAction(id);
                    if (data != null)
                        request.CreateResponse().BodyJson(data).SendAsync();
                    else
                        request.CreateResponse().StatusError()
                            .Body("Cannot find the action with the specified ID.")
                            .SendAsync();
                });

            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/monkey/move", request =>
            {
                var stringPosition = request.QueryParametersDict["position"].FirstOrDefault();
                if (!int.TryParse(stringPosition, out var position))
                {
                    request.CreateResponse()
                        .StatusError()
                        .Body("Position is not a valid integer.")
                        .SendAsync();
                    return;
                }

                request.CreateResponse().BodyJson(MonkeyMove(position)).SendAsync();
            });
            
            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/monkey/grab", request =>
            {
                request.CreateResponse().BodyJson(MonkeyGrabItem()).SendAsync();
            });
            
            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/monkey/drop", request =>
            {
                request.CreateResponse().BodyJson(MonkeyDropItem()).SendAsync();
            });
            
            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/monkey/climb", request =>
            {
                request.CreateResponse().BodyJson(MonkeyClimb()).SendAsync();
            });
        }

        private SceneData GetSceneData()
        {
            return new SceneData
            {
                IsCompleted = IsTaskCompleted,
                Monkey = monkey.GenerateData(),
                Items = DiscoveredItems
                    .Select(item => item.GenerateData())
                    .OrderBy(data => data.Id)
                    .ToArray(),
                Floors = DiscoveredFloors
                    .Select(floor => floor.GenerateData())
                    .OrderBy(data => data.Y)
                    .ToArray()
            };
        }

        private ActionData SearchAction(int id)
        {
            return _actions.Get(id.ToString()) as ActionData;
        }

        private void DispatchAction(ActionData action)
        {
            _actions.Set(action.Id.ToString(), action, new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromSeconds(45)
            });
            monkey.AssignAction(action);
            // Most actions finish condition checks within two frames.
            Thread.Sleep(TimeSpan.FromSeconds(_actionWaitingTime));
        }

        private ActionData MonkeyMove(int x)
        {
            var action = new MonkeyMoveAction
            {
                Id = Interlocked.Increment(ref _actionNextId),
                GoalPosition = x,
            };
            DispatchAction(action);
            return action;
        }

        private ActionData MonkeyGrabItem()
        {
            var action = new MonkeyGrabAction()
            {
                Id = Interlocked.Increment(ref _actionNextId),
            };
            DispatchAction(action);
            return action;
        }
        
        private ActionData MonkeyDropItem()
        {
            var action = new MonkeyDropAction()
            {
                Id = Interlocked.Increment(ref _actionNextId),
            };
            DispatchAction(action);
            return action;
        }
        
        private ActionData MonkeyClimb()
        {
            var action = new MonkeyClimbAction()
            {
                Id = Interlocked.Increment(ref _actionNextId),
            };
            DispatchAction(action);
            return action;
        }
    }
}