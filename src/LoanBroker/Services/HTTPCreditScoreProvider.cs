using System.Net.Http.Json;
using ClientMessages;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider : ICreditScoreProvider
{
    readonly string lambdaUrl = Environment.GetEnvironmentVariable("LAMBDA_URL")
                                ?? "https://score.lambda-url.us-east-1.localhost.localstack.cloud:4566";

    public async Task<int> Score(Prospect prospect, string requestId)
    {
        using var httpClient = CreateHttpClient();
        var requestRecord = new ScoreRequest(prospect.SSN, requestId);
        var httpResponseMessage = await httpClient.PostAsync(lambdaUrl, JsonContent.Create(requestRecord));
        var scoreResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ScoreResponse>();
        return scoreResponse!.score;
    }

    HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        return new HttpClient(handler);
    }
}

record ScoreRequest(string SSN, string requestId);

record ScoreResponse(int score);