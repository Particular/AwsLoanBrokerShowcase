
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapPost("/score", (ScoreRequest request) =>
{

    if (Regex.IsMatch(request.SSN, @"^\d{3}-\d{2}-\d{4}$")) {
        Console.WriteLine("hello there");
        var score = Random.Shared.Next(300, 900);
        return new ScoreResponse(request.Id, score);
    }

    throw new Exception($"Invalid SSN {request.SSN} for request id {request.Id}" );

});
app.Run();

record ScoreRequest(string SSN, string Id);

record ScoreResponse(string Id, int Score);
