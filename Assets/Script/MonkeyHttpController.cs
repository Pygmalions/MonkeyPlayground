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
    public partial class MonkeyHttpController : MonoBehaviour
    {
        public RestServer.RestServer server;

        /// <summary>
        /// Monkey controlled by this controller.
        /// There might be multiple monkeys in the scene,
        /// but only one is controlled by this controller.
        /// </summary>
        [Tooltip("The monkey controlled by this controller.")]
        public Monkey monkey;

        [Header("Scene Information")]
        public string sceneName = "Normal Scene";

        [TextArea(3, 10)] 
        public string sceneDescription = "This is a classical monkey-banana problem.";

        public Item[] DiscoveredItems { get; private set; }

        public Floor[] DiscoveredFloors { get; private set; }
        
        /// <summary>
        /// This flag indicates whether the task is completed.
        /// It is set by the banana item.
        /// </summary>
        public bool IsTaskCompleted { get; internal set; }
        
        private readonly MemoryCache _actions = new("ActionDataCache");

        private static int _actionNextId = 0;
        
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

        private float _actionWaitingTime;
        
        private void Start()
        {
            IsTaskCompleted = false;
            _actionWaitingTime = Time.fixedDeltaTime * 2;
            
            ScanPerceptibleObjects();

            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/scene/status", RequestSceneStatus);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/scene/description", RequestSceneDescription);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/action/status", RequestActionData);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/move", RequestMonkeyMove);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/grab", RequestMonkeyGrabItem);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/drop", RequestMonkeyDropItem);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/climb", RequestMonkeyClimb);

            server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, 
                "/scene/restart", RequestSceneRestart);
            server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/scene/switch", RequestSceneSwitch);
        }

        private void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
        
        private void SwitchScene(string scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
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
            monkey.ongoingAction = action;
            // Most actions finish condition checks within two frames.
            Thread.Sleep(TimeSpan.FromSeconds(_actionWaitingTime));
        }
    }
}