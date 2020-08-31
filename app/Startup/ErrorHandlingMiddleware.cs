using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace loan_api
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _telemetryClient;

        public ErrorHandlingMiddleware(RequestDelegate next, TelemetryClient telemetryClient)
        {
            _next = next;
            _telemetryClient = telemetryClient;
        }

        public async Task Invoke(HttpContext context, IHostEnvironment env)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _telemetryClient.TrackException(ex);
                await HandleExceptionAsync(context, ex, env, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                await HandleExceptionAsync(context, ex, env, HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex, IHostEnvironment env, HttpStatusCode statusCode)
        {
            var result = JsonConvert.SerializeObject(
                new
                {
                    title = "One or more unexpected errors occurred.",
                    status = (int)HttpStatusCode.InternalServerError,
                    exception = env.IsProduction() ? null : ex
                }, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
