using WarrantySystem.Shared.Common;
using System.Text.Json;

namespace WarrantySystem.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log error using both built-in logger and custom error logger
                _logger.LogError(ex, "An unhandled exception has occurred.");
                DateTime now = DateTime.Now;

                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                string monthDirectory = Path.Combine(logsDirectory, now.ToString("yyyy-MM"));

                Directory.CreateDirectory(monthDirectory);

                string logFilePath = Path.Combine(monthDirectory, now.ToString("dd") + ".txt");

                string logMessage = $"[{now}] Error: {ex.Message}\n{ex.StackTrace}\n\n";

                File.AppendAllText(logFilePath, logMessage);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = ApiResponseFactory.Fail(exception, "Đã có lỗi xảy ra.");

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
