using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CreditBureau;

public class RequestLoggingMiddleware : IFunctionsWorkerMiddleware
{
    public const string BufferedBodyBytesKey = "BufferedRequestBodyBytes";

    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpReqData = await context.GetHttpRequestDataAsync();

        if (httpReqData != null)
        {
            _logger.LogInformation("HTTP {Method} {Url}", httpReqData.Method, httpReqData.Url);

            var contentType = httpReqData.Headers.TryGetValues("Content-Type", out var ct) ? string.Join(", ", ct) : "<missing>";
            var contentLength = httpReqData.Headers.TryGetValues("Content-Length", out var cl) ? string.Join(", ", cl) : "<missing>";
            var transferEncoding = httpReqData.Headers.TryGetValues("Transfer-Encoding", out var te) ? string.Join(", ", te) : "<missing>";

            _logger.LogInformation(
                "Headers: Content-Type={ContentType}; Content-Length={ContentLength}; Transfer-Encoding={TransferEncoding}",
                contentType, contentLength, transferEncoding);

            _logger.LogInformation("Query: {Query}", httpReqData.Url.Query);
            _logger.LogInformation("Body stream: CanRead={CanRead}; CanSeek={CanSeek}", httpReqData.Body.CanRead, httpReqData.Body.CanSeek);

            byte[] bodyBytes;

            if (httpReqData.Body.CanSeek)
            {
                httpReqData.Body.Position = 0;
                using var ms = new MemoryStream();
                await httpReqData.Body.CopyToAsync(ms);
                bodyBytes = ms.ToArray();
                httpReqData.Body.Position = 0; // safe rewind
            }
            else
            {
                // Non-seekable stream: reading it here would normally consume it.
                // We DO read it for logging, but we also stash the bytes so the function can use them.
                using var ms = new MemoryStream();
                await httpReqData.Body.CopyToAsync(ms);
                bodyBytes = ms.ToArray();

                context.Items[BufferedBodyBytesKey] = bodyBytes;

                _logger.LogWarning(
                    "Request body stream is not seekable. Middleware buffered {Length} bytes into FunctionContext.Items['{Key}']. " +
                    "Function should read from context.Items instead of req.Body to avoid 'empty body' issues.",
                    bodyBytes.Length, BufferedBodyBytesKey);
            }

            _logger.LogInformation("Body bytes read: {Length}", bodyBytes.Length);

            const int maxPreviewBytes = 2048;
            var previewBytes = bodyBytes.Length <= maxPreviewBytes ? bodyBytes : bodyBytes[..maxPreviewBytes];
            var previewText = Encoding.UTF8.GetString(previewBytes);

            _logger.LogInformation("Body preview (first {PreviewLength} bytes{Truncated}): {BodyPreview}",
                previewBytes.Length,
                bodyBytes.Length > maxPreviewBytes ? ", truncated" : string.Empty,
                previewText);
        }

        await next(context);
    }
}