﻿using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ClientMessages;
using Microsoft.Extensions.Logging;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider(ILogger<HTTPCreditScoreProvider> logger) : ICreditScoreProvider
{
    readonly string functionUrl = Environment.GetEnvironmentVariable("CREDIT_BUREAU_URL")
                                ?? "http://creditbureau:8080/api/score";

    public async Task<int> Score(Prospect prospect, string requestId)
    {
        using var httpClient = new HttpClient();
        var requestRecord = new ScoreRequest(prospect.SSN, requestId);
        var json = System.Text.Json.JsonSerializer.Serialize(requestRecord);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        logger.LogInformation($"Sending request to credit bureau: {content} via {functionUrl}");

        var httpResponseMessage = await httpClient.PostAsync(functionUrl, content);

        httpResponseMessage.EnsureSuccessStatusCode();
        var scoreResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ScoreResponse>();

        return scoreResponse!.Score;
    }
}

record ScoreRequest(
    [property: JsonPropertyName("ssn")] string SSN,
    [property: JsonPropertyName("requestId")] string RequestId);

record ScoreResponse(
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("history")] int History,
    [property: JsonPropertyName("SSN")] string SSN,
    [property: JsonPropertyName("request_id")] string RequestId);
