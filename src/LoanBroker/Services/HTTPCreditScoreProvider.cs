using System.Net.Http.Json;
using System.Text;
using ClientMessages;

namespace LoanBroker.Services;

public class HTTPCreditScoreProvider : ICreditScoreProvider
{
    public int Score(Prospect prospelct)
    {
        return getScore().GetAwaiter().GetResult();
    }

    private async Task<int> getScore()
    {
        using var httpClient =  new HttpClient();

        var requestBody = new StringContent("{\n\"SSN\": \"123-12-1234\",\n\"RequestId\": \"123-12-1234\"\n}",Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("http://localhost:5291/score", requestBody);
        var score = await response.Content.ReadFromJsonAsync<ScoreResponse>();

        return score.score;
    }
}


record ScoreResponse(int score);