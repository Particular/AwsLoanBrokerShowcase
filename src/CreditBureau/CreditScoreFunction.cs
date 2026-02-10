using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Credit score request received");

        ScoreRequest? scoreRequest = null;

        if (context.Items.TryGetValue(RequestLoggingMiddleware.BufferedBodyBytesKey, out var bufferedObj) &&
            bufferedObj is byte[] bufferedBytes)
        {
            _logger.LogInformation("Reading request body from FunctionContext buffered bytes: {Length}", bufferedBytes.Length);

            try
            {
                scoreRequest = JsonSerializer.Deserialize<ScoreRequest>(bufferedBytes);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize buffered request body as ScoreRequest");
            }
        }
        else
        {
            scoreRequest = await req.ReadFromJsonAsync<ScoreRequest>();
        }

        if (scoreRequest is null || string.IsNullOrEmpty(scoreRequest.SSN))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = "Invalid request body or missing SSN" });
            return badRequest;
        }

        if (!SsnRegex.IsMatch(scoreRequest.SSN))
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

        var response = req.CreateResponse(HttpStatusCode.OK);

        var scoreResponse = new ScoreResponse(
            GetRandomInt(MinScore, MaxScore),
            GetRandomInt(1, 30),
            scoreRequest.SSN,
            scoreRequest.RequestId);

        await response.WriteAsJsonAsync(scoreResponse);

        return response;
    }

    private static int GetRandomInt(int min, int max)
    {
        return min + _random.Next(max - min);
    }
}

public record ScoreRequest(
    [property: JsonPropertyName("ssn")] string SSN,
    [property: JsonPropertyName("requestId")] string RequestId);

public record ScoreResponse(
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("history")] int History,
    [property: JsonPropertyName("SSN")] string SSN,
    [property: JsonPropertyName("request_id")] string RequestId);