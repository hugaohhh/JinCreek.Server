using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Auth
{
    // https://tech-lab.sios.jp/archives/16110 , https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestId = Guid.NewGuid().ToString();
            try
            {
                NLog.NestedDiagnosticsContext.Push(requestId);

                var request = await FormatRequest(httpContext.Request);
                _logger.LogInformation("START {0}", request);


                var originalBodyStream = httpContext.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    httpContext.Response.Body = responseBody;
                    sw.Start();
                    await _next(httpContext);
                    sw.Stop();
                    var response = await FormatResponse(httpContext.Response);

                    await responseBody.CopyToAsync(originalBodyStream);

                    _logger.LogInformation("END {0} {1}ms", response, sw.ElapsedMilliseconds);
                }
                NLog.LogManager.GetLogger("access_log").Info("ACCCESS");
            }
            finally
            {
                NLog.NestedDiagnosticsContext.Pop();
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            string bodyAsText;
            request.EnableBuffering();

            using (StreamReader reader = new StreamReader(request.Body, System.Text.Encoding.UTF8, true, 1024, true))
            {
                bodyAsText = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            return $"m={request.Method},ct={request.ContentType},host={request.Host},path={request.Path},q={request.QueryString},b={bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"c={response.StatusCode},b={text}";
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
