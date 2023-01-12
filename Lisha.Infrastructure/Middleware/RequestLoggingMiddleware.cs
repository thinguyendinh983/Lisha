using Lisha.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using System.Text;

namespace Lisha.Infrastructure.Middleware
{
    public class RequestLoggingMiddleware : IMiddleware
    {
        private readonly IDateTime _dateTime;

        public RequestLoggingMiddleware(IDateTime dateTime)
        {
            _dateTime = dateTime;
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            LogContext.PushProperty("RequestTimeUTC", _dateTime.Now);
            string requestBody = string.Empty;
            if (httpContext.Request.Path.ToString().Contains("tokens"))
            {
                requestBody = "[Redacted] Contains Sensitive Information.";
            }
            else
            {
                var request = httpContext.Request;

                if (!string.IsNullOrEmpty(request.ContentType)
                    && request.ContentType.StartsWith("application/json"))
                {
                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 4096, true);
                    requestBody = await reader.ReadToEndAsync();

                    // rewind for next middleware.
                    request.Body.Position = 0;
                }
            }

            LogContext.PushProperty("RequestBody", requestBody);
            Log.ForContext("RequestHeaders", httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
               .ForContext("RequestBody", requestBody)
               .Information("HTTP {RequestMethod} Request sent to {RequestPath}", httpContext.Request.Method, httpContext.Request.Path);
            await next(httpContext);
        }
    }
}
