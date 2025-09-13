using System.Linq;
using System.Threading;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using MonkeyPlayground.Objects;
using UnityEngine;
using RestServer;

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
        
        [Multiline(int.MaxValue)]
        public string sceneDescription = "This is a classical monkey-banana problem.";

        public Item[] DiscoveredItems { get; private set; }

        public Floor[] DiscoveredFloors { get; private set; }

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
        }

        private void Start()
        {
            ScanPerceptibleObjects();

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/scene/status",
                request => { request.CreateResponse().BodyJson(GetSceneData()).SendAsync(); });

            server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/scene/description",
                request => { request.CreateResponse().BodyJson(new
                {
                    Name = sceneName,
                    Description = sceneDescription
                }).SendAsync(); });
            
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

                request.CreateResponse().BodyJson(MoveMonkey(position)).SendAsync();
            });
        }

        private SceneData GetSceneData()
        {
            return new SceneData
            {
                IsCompleted = false,
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

        private ActionData MoveMonkey(int x)
        {
            var action = new MonkeyMovingAction()
            {
                Id = Interlocked.Increment(ref _actionNextId),
                GoalPosition = x
            };
            // Assign the action to the monkey.
            return monkey.AssignAction(action);
        }
    }
}