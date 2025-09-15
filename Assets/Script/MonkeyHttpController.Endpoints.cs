using System.Linq;
using System.Threading;
using MonkeyPlayground.Data;
using MonkeyPlayground.Data.Actions;
using RestServer;

namespace MonkeyPlayground;

public partial class MonkeyHttpController
{
    private void RequestSceneStatus(RestRequest request)
    {
        var data = new SceneData
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
        if (!int.TryParse(stringPosition, out var position))
        {
            request.CreateResponse()
                .StatusError()
                .Body("Position is not a valid integer.")
                .SendAsync();
            return;
        }
        var action = new MonkeyMoveAction
        {
            Id = Interlocked.Increment(ref _actionNextId),
            GoalPosition = position,
        };
        DispatchAction(action);
        request.CreateResponse().BodyJson(action).SendAsync();
    }
    
    private void RequestMonkeyClimb(RestRequest request)
    {
        var action = new MonkeyClimbAction
        {
            Id = Interlocked.Increment(ref _actionNextId),
        };
        DispatchAction(action);
        request.CreateResponse().BodyJson(action).SendAsync();
    }
    
    private void RequestMonkeyGrabItem(RestRequest request)
    {
        var action = new MonkeyGrabAction()
        {
            Id = Interlocked.Increment(ref _actionNextId),
        };
        DispatchAction(action);
        request.CreateResponse().BodyJson(action).SendAsync();
    }
    
    private void RequestMonkeyDropItem(RestRequest request)
    {
        var action = new MonkeyDropAction()
        {
            Id = Interlocked.Increment(ref _actionNextId),
        };
        DispatchAction(action);
        request.CreateResponse().BodyJson(action).SendAsync();
    }

    private void RequestSceneRestart(RestRequest request)
    {
        ThreadingHelper.Instance.ExecuteAsync(RestartScene);
        request.CreateResponse().SendAsync();
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
        ThreadingHelper.Instance.ExecuteAsync(() => SwitchScene(id));
        request.CreateResponse().SendAsync();
    }
}