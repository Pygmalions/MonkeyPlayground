using System;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using MonkeyPlayground.Models;
using MonkeyPlayground.Models.ActionModel;
using MonkeyPlayground.Models.ActionModel.Actions;
using MonkeyPlayground.Objects;
using RestServer.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyPlayground.Controllers
{
    [DisallowMultipleComponent,
     RequireComponent(typeof(RestServer.Runtime.RestServer), typeof(Monkey))]
    public class MonkeyNetworkController : MonoBehaviour
    {
        private static int _storedPort = 9000;
        
        private readonly MemoryCache _actions = new(new MemoryCacheOptions());

        private RestServer.Runtime.RestServer _server;
        private Monkey _monkey;

        [Header("Scene Information")]
        [SerializeField] 
        public string sceneName = "Normal Scene";
        [SerializeField, TextArea(3, 10)] 
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

        public GameObject settingsPanel;

        private TimeSpan _actionWaitingTime;
        
        private TextMeshProUGUI _displayTitle;
        private TextMeshProUGUI _displayDescription;
        private TextMeshProUGUI _displayContent;
        private TextMeshProUGUI _displayPort;
        private TextMeshProUGUI _displayStatus;
        private TextMeshProUGUI _displayScene;
        private TMP_InputField _inputPort;

        private void Awake()
        {
            _monkey = GetComponent<Monkey>();
            _server = GetComponent<RestServer.Runtime.RestServer>();
            
            _displayTitle = GameObject.Find("Text_Title").GetComponent<TextMeshProUGUI>();
            _displayDescription = GameObject.Find("Text_Description").GetComponent<TextMeshProUGUI>();
            _displayContent = GameObject.Find("Text_Content").GetComponent<TextMeshProUGUI>();
            _displayPort = GameObject.Find("Text_Port").GetComponent<TextMeshProUGUI>();
            _displayStatus = GameObject.Find("Text_Status").GetComponent<TextMeshProUGUI>();
            _displayScene = GameObject.Find("Text_Scene").GetComponent<TextMeshProUGUI>();
            
            
            if (settingsPanel)
            {
                _inputPort = settingsPanel.transform.Find("InputField_Port").GetComponent<TMP_InputField>();
                _inputPort.text = _server.port.ToString();
            }
        }
        
        private void Start()
        {
            _server.port = _storedPort;
            _displayPort.text = $"Port: {_server.port}";
            _displayScene.text = $"Scene: {SceneManager.GetActiveScene().buildIndex}";
            
            _actionWaitingTime = TimeSpan.FromSeconds(Time.fixedDeltaTime * 2);
            
            ScanPerceptibleObjects();

            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/scene/status", RequestSceneStatus);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/scene/description", RequestSceneDescription);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.GET, "/action/status", RequestActionData);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/move", RequestMonkeyMove);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/grab", RequestMonkeyGrabItem);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/drop", RequestMonkeyDropItem);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/climb-up", RequestMonkeyClimbUp);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/monkey/climb-down", RequestMonkeyClimbDown);

            _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                "/scene/restart", RequestSceneRestart);
            _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST,
                "/scene/switch", RequestSceneSwitch);
            
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/display/title", RequestSetDisplayTitle);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/display/description", RequestSetDescription);
            _server.EndpointCollection.RegisterEndpoint(
                HttpMethod.POST, "/display/content", RequestSetDisplayContent);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            if (!_monkey.HasReachedBanana)
            {
                _displayStatus.text = "Running";
                _displayStatus.color = Color.gold;
            }
            else
            {
                _displayStatus.text = "Completed";
                _displayStatus.color = Color.forestGreen;
            }
        }
        
        public void ApplyPortFromSettingMenu()
        {
            ChangeServerPort(int.Parse(_inputPort.text));
            _displayPort.text = $"Port: {_server.port}";
        }

        public void ChangeServerPort(int port)
        {
            if (_server.IsStarted)
                _server.StopServer();
            _server.port = port;
            _storedPort = port;
            _server.StartServer();
        }
        
        [CanBeNull]
        private ActionData SearchAction(int id)
        {
            if (_actions.Get(id.ToString()) is ActionData data)
                return data;
            return null;
        }

        private void RequestSceneStatus(RestRequest request)
        {
            var data = new SceneData
            {
                IsCompleted = _monkey.HasReachedBanana,
                Monkey = _monkey.GenerateData(),
                Items = DiscoveredItems
                    .Select(item => item.GenerateData())
                    .OrderBy(data => data.Id)
                    .ToArray(),
                Floors = DiscoveredFloors
                    .Select(floor => floor.GenerateData())
                    .ToArray()
            };
            request.CreateResponse().BodyJson(data).SendAsync();
        }

        private void RequestSceneDescription(RestRequest request)
        {
            var data = new
            {
                Name = sceneName,
                Description = sceneDescription
            };
            request.CreateResponse().BodyJson(data).SendAsync();
        }

        private void RequestActionData(RestRequest request)
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
        }

        private void RequestMonkeyMove(RestRequest request)
        {
            var stringPosition = request.QueryParametersDict["position"].FirstOrDefault();
            if (!float.TryParse(stringPosition, out var position))
            {
                request.CreateResponse()
                    .StatusError()
                    .Body("Position is not a valid number.")
                    .SendAsync();
                return;
            }

            var action = new MonkeyMoveAction();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _monkey.Move(position, result => action.Result = result);
            });
            _actions.Set(action.Id.ToString(), action, TimeSpan.FromSeconds(45));
            Thread.Sleep(_actionWaitingTime);
            request.CreateResponse().BodyJson(action).SendAsync();
        }

        private void RequestMonkeyClimbUp(RestRequest request)
        {
            var action = new MonkeyClimbUpAction();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _monkey.ClimbUp(result => action.Result = result);
            });
            _actions.Set(action.Id.ToString(), action, TimeSpan.FromSeconds(45));
            Thread.Sleep(_actionWaitingTime);
            request.CreateResponse().BodyJson(action).SendAsync();
        }
        
        private void RequestMonkeyClimbDown(RestRequest request)
        {
            var action = new MonkeyClimbDownAction();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _monkey.ClimbDown(result => action.Result = result);
            });
            _actions.Set(action.Id.ToString(), action, TimeSpan.FromSeconds(45));
            Thread.Sleep(_actionWaitingTime);
            request.CreateResponse().BodyJson(action).SendAsync();
        }

        private void RequestMonkeyGrabItem(RestRequest request)
        {
            var action = new MonkeyGrabAction();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _monkey.GrabItem(result => action.Result = result);
            });
            _actions.Set(action.Id.ToString(), action, TimeSpan.FromSeconds(45));
            Thread.Sleep(_actionWaitingTime);
            request.CreateResponse().BodyJson(action).SendAsync();
        }

        private void RequestMonkeyDropItem(RestRequest request)
        {
            var action = new MonkeyDropAction();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _monkey.DropItem(result => action.Result = result);
            });
            _actions.Set(action.Id.ToString(), action, TimeSpan.FromSeconds(45));
            Thread.Sleep(_actionWaitingTime);
            request.CreateResponse().BodyJson(action).SendAsync();
        }

        private void RequestSceneRestart(RestRequest request)
        {
            request.CreateResponse().SendAsync();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
                ActionData.ResetGlobalIdCounter();
            });
        }
        
        private void RequestSceneSwitch(RestRequest request)
        {
            var stringId = request.QueryParametersDict["id"].FirstOrDefault();
            if (!int.TryParse(stringId, out var id))
            {
                request.CreateResponse().StatusError()
                    .Body("Scene ID is not a valid integer.")
                    .SendAsync();
                return;
            }
            request.CreateResponse().SendAsync();
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                SceneManager.LoadScene(id, LoadSceneMode.Single);
                ActionData.ResetGlobalIdCounter();
            });
        }

        private void RequestSetDisplayTitle(RestRequest request)
        {
            var title = request.Body;
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _displayTitle.text = title;
            });
            request.CreateResponse().SendAsync();
        }
        
        private void RequestSetDescription(RestRequest request)
        {
            var description = request.Body;
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _displayDescription.text = description;
            });
            request.CreateResponse().SendAsync();
        }
        
        private void RequestSetDisplayContent(RestRequest request)
        {
            var content = request.Body;
            ThreadingHelper.Instance.ExecuteAsync(() =>
            {
                _displayContent.text = content;
            });
            request.CreateResponse().SendAsync();
        }
    }
}