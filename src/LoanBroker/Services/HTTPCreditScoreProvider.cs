using System.Net.Http.Json;
using System.Text;
using ClientMessages;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider : ICreditScoreProvider
{
    public int Score(Prospect prospect)
    {
        using var httpClient =  new HttpClient();
        var url = "https://score.lambda-url.us-east-1.localhost.localstack.cloud:4566";
        var requestRecord = new ScoreRequest("123-12-1234", "ABC");
        return httpClient.PostAsync(url, JsonContent.Create(requestRecord))
            .ContinueWith(task => task.Result.Content.ReadFromJsonAsync<ScoreResponse>()).Unwrap()
            .ContinueWith(task => task.Result.score)
            .Result;
    }

}

record ScoreRequest(string SSN, string requestId);
record ScoreResponse(int score);