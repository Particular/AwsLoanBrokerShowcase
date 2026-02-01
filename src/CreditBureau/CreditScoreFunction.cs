using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CreditBureau;

public class CreditScoreFunction
{
    private readonly ILogger<CreditScoreFunction> _logger;
    private static readonly Random _random = new();
    private const int MinScore = 300;
    private const int MaxScore = 900;
    private static readonly Regex SsnRegex = new(@"^\d{3}-\d{2}-\d{4}$", RegexOptions.Compiled);

    public CreditScoreFunction(ILogger<CreditScoreFunction> logger)
    {
        _logger = logger;
    }

    [Function("score")]
    public async Task<HttpResponseData> Score(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Credit score request received");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var scoreRequest = JsonSerializer.Deserialize<ScoreRequest>(requestBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (scoreRequest == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid request body" });
            return badRequest;
        }

        if (SsnRegex.IsMatch(scoreRequest.SSN))
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var scoreResponse = new ScoreResponse
            {
                SSN = scoreRequest.SSN,
                Score = GetRandomInt(MinScore, MaxScore),
                History = GetRandomInt(1, 30),
                RequestId = scoreRequest.RequestId
            };

            await response.WriteAsJsonAsync(scoreResponse);
            return response;
        }
        else
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new
            {
                SSN = scoreRequest.SSN,
                RequestId = scoreRequest.RequestId,
                Error = "Invalid SSN format"
            });
            return badResponse;
        }
    }

    private static int GetRandomInt(int min, int max)
    {
        return min + _random.Next(max - min);
    }
}

public record ScoreRequest
{
    public string SSN { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
}

public record ScoreResponse
{
    public string SSN { get; init; } = string.Empty;
    public int Score { get; init; }
    public int History { get; init; }
    public string RequestId { get; init; } = string.Empty;
}
