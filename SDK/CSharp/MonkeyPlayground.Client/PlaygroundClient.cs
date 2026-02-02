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

    public ResiliencePipeline Pipeline { get; init; }

    public async Task DisplayWindowTitle(string title)
    {
        await Pipeline.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/display/title")
                .AddStringBody(title, ContentType.Plain), cancellation);
        }, CancellationToken.None);
    }

    public async Task DisplayWindowDescription(string description)
    {
        await Pipeline.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/display/description")
                .AddStringBody(description, ContentType.Plain), cancellation);
        }, CancellationToken.None);
    }

    public async Task DisplayWindowContent(string content)
    {
        await Pipeline.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/display/content")
                .AddStringBody(content, ContentType.Plain), cancellation);
        }, CancellationToken.None);
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
        Pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Linear,
                Delay = TimeSpan.FromMilliseconds(100),
            })
            .Build();
    }

    public async Task RestartScene()
    {
        await Pipeline.ExecuteAsync(
            async cancellation => { await _client.PostAsync(new RestRequest("/scene/restart"), cancellation); },
            CancellationToken.None);
    }

    public async Task SwitchScene(int sceneId)
    {
        await Pipeline.ExecuteAsync(async cancellation =>
        {
            await _client.PostAsync(new RestRequest("/scene/switch")
                .AddQueryParameter("id", sceneId), cancellation);
        }, CancellationToken.None);
    }

    public async Task<SceneData> GetSceneStatus()
    {
        return await Pipeline.ExecuteAsync(async cancellation => await _client.GetAsync<SceneData>(
            new RestRequest("/scene/status"), cancellation), CancellationToken.None);
    }

    public async Task<JsonObject> GetSceneDescription()
    {
        return await Pipeline.ExecuteAsync(async cancellation =>
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
        var response = await Pipeline.ExecuteAsync(async cancellation =>
            await _client.GetAsync(request, cancellation), CancellationToken.None);
        var data = JsonNode.Parse(response.Content!)!.AsObject();
        return ParseActionData(data);
    }

    public async Task<MonkeyMoveAction> Move(float position)
    {
        var request = new RestRequest("/monkey/move")
            .AddQueryParameter("position", position);
        var response = (await Pipeline.ExecuteAsync(async cancellation =>
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
        var response = (await Pipeline.ExecuteAsync(async cancellation =>
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
        var response = (await Pipeline.ExecuteAsync(async cancellation =>
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
        var response = (await Pipeline.ExecuteAsync(async cancellation =>
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
        var response = (await Pipeline.ExecuteAsync(async cancellation =>
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