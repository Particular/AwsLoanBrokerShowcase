using System.Net.Http.Json;
using System.Text;
using ClientMessages;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider : ICreditScoreProvider
{
    public int Score(Prospect prospect)
    {
        return getScore().GetAwaiter().GetResult();
    }

    private async Task<int> getScore()
    {
        using var httpClient =  new HttpClient();
        var url = "https://score.lambda-url.us-east-1.localhost.localstack.cloud:4566/?SSN=123-12-1234&RequestId=123";
         var response = await httpClient.GetAsync(url);
        var score = await response.Content.ReadFromJsonAsync<ScoreResponse>();
        Console.WriteLine(score);

        return score.score;
    }
}


record ScoreResponse(int score);