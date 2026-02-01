﻿using System.Net.Http.Json;
using ClientMessages;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider : ICreditScoreProvider
{
    readonly string functionUrl = Environment.GetEnvironmentVariable("CREDIT_BUREAU_URL")
                                ?? "http://creditbureau:8080/api/score";

    public async Task<int> Score(Prospect prospect, string requestId)
    {
        using var httpClient = new HttpClient();
        var requestRecord = new ScoreRequest(prospect.SSN, requestId);
        var httpResponseMessage = await httpClient.PostAsync(functionUrl, JsonContent.Create(requestRecord));
        httpResponseMessage.EnsureSuccessStatusCode();
        var scoreResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ScoreResponse>();
        return scoreResponse!.Score;
    }
}

record ScoreRequest(string SSN, string RequestId);

record ScoreResponse(int Score, int History, string SSN, string RequestId);
