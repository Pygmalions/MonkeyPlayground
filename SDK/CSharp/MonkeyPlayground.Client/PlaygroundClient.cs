using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MonkeyPlayground.Client.Models;
using MonkeyPlayground.Client.Models.Actions;
using Polly;
using Polly.Retry;
using RestSharp;
using RestSharp.Serializers.Json;

namespace MonkeyPlayground.Client;

public class PlaygroundClient
{
    private readonly RestClient _client;

    public AsyncRetryPolicy RetryPolicy { get; set; }
    
    public string Title
    {
        get;
        set
        {
            field = value;
            _client.Post(new RestRequest("/display/title")
                .AddStringBody(value, ContentType.Plain));
        }
    } = "";

    public string Description
    {
        get;
        set
        {
            field = value;
            _client.Post(new RestRequest("/display/description")
                .AddStringBody(value, ContentType.Plain));
        }
    } = "";

    public async Task DisplayContent(string content)
    {
        await _client.PostAsync(new RestRequest("/display/content")
            .AddStringBody(content, ContentType.Plain));
    }

    public PlaygroundClient(Uri baseUrl)
    {
        _client = new RestClient(new RestClientOptions(baseUrl),
            configureSerialization: config =>
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter());
                config.UseSystemTextJson(options);
            });
        RetryPolicy = Policy.Handle<HttpRequestException>()
            .RetryAsync(3, async (_, times, _) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100 * times));
            });
    }

    public async Task RestartScene()
    {
        await RetryPolicy.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/scene/restart"), cancellation);
        }, CancellationToken.None);
    }
    
    public async Task SwitchScene(int sceneId)
    {
        await RetryPolicy.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/scene/switch")
                .AddQueryParameter("id",  sceneId), cancellation);
        }, CancellationToken.None);
    }

    public async Task<SceneData> GetSceneStatus()
    {
        return await RetryPolicy.ExecuteAsync(async cancellation => await _client.GetAsync<SceneData>(
            new RestRequest("/scene/status"), cancellation), CancellationToken.None);
    }

    public async Task<JsonObject> GetSceneDescription()
    {
        return await RetryPolicy.ExecuteAsync(async cancellation =>
        {
            var response = await _client.GetAsync(
                new RestRequest("/scene/description"), cancellation);
            return JsonNode.Parse(response.Content!)!.AsObject();
        }, CancellationToken.None);
    }

    public async Task<ActionData> GetActionStatus(int actionId)
    {
        var request = new RestRequest("/action/status")
            .AddQueryParameter("id", actionId);
        var response = await RetryPolicy.ExecuteAsync(async cancellation =>
                await _client.GetAsync(request, cancellation), CancellationToken.None);
        var data = JsonNode.Parse(response.Content!)!.AsObject();
        return ParseActionData(data);
    }

    public async Task<MonkeyMoveAction> Move(float position)
    {
        var request = new RestRequest("/monkey/move")
            .AddQueryParameter("position", position);
        var response = (await RetryPolicy.ExecuteAsync(async cancellation =>
            await _client.PostAsync<MonkeyMoveAction>(request, cancellation), CancellationToken.None))!;
        if (response.IsCompleted())
            return response;
        var id = response.Id;
        while (true)
        {
            var result = await GetActionStatus(id);
            if (result.IsCompleted())
                return (MonkeyMoveAction)result;
        }
    }

    public async Task<MonkeyGrabAction> Grab()
    {
        var request = new RestRequest("/monkey/grab");
        var response = (await RetryPolicy.ExecuteAsync(async cancellation =>
            await _client.PostAsync<MonkeyGrabAction>(request, cancellation), CancellationToken.None))!;
        if (response.IsCompleted())
            return response;
        var id = response.Id;
        while (true)
        {
            var result = await GetActionStatus(id);
            if (result.IsCompleted())
                return (MonkeyGrabAction)result;
        }
    }

    public async Task<MonkeyDropAction> Drop()
    {
        var request = new RestRequest("/monkey/drop");
        var response = (await RetryPolicy.ExecuteAsync(async cancellation =>
            await _client.PostAsync<MonkeyDropAction>(request, cancellation), CancellationToken.None))!;
        if (response.IsCompleted())
            return response;
        var id = response.Id;
        while (true)
        {
            var result = await GetActionStatus(id);
            if (result.IsCompleted())
                return (MonkeyDropAction)result;
        }
    }

    public async Task<MonkeyClimbUpAction> ClimbUp()
    {
        var request = new RestRequest("/monkey/climb-up");
        var response = (await RetryPolicy.ExecuteAsync(async cancellation =>
            await _client.PostAsync<MonkeyClimbUpAction>(request, cancellation), CancellationToken.None))!;
        if (response.IsCompleted())
            return response;
        var id = response.Id;
        while (true)
        {
            var result = await GetActionStatus(id);
            if (result.IsCompleted())
                return (MonkeyClimbUpAction)result;
        }
    }
    
    public async Task<MonkeyClimbDownAction> ClimbDown()
    {
        var request = new RestRequest("/monkey/climb-down");
        var response = (await RetryPolicy.ExecuteAsync(async cancellation =>
            await _client.PostAsync<MonkeyClimbDownAction>(request, cancellation), CancellationToken.None))!;
        if (response.IsCompleted())
            return response;
        var id = response.Id;
        while (true)
        {
            var result = await GetActionStatus(id);
            if (result.IsCompleted())
                return (MonkeyClimbDownAction)result;
        }
    }

    private ActionData ParseActionData(JsonObject data)
    {
        var name = data["Name"]!.GetValue<string>();
        return name switch
        {
            "Move" => data.Deserialize<MonkeyMoveAction>()!,
            "Climb Up" => data.Deserialize<MonkeyClimbUpAction>()!,
            "Climb Down" => data.Deserialize<MonkeyClimbDownAction>()!,
            "Grab Item" => data.Deserialize<MonkeyGrabAction>()!,
            "Drop Item" => data.Deserialize<MonkeyDropAction>()!,
            _ => throw new NotSupportedException($"Unsupported action name: {name}")
        };
    }
}