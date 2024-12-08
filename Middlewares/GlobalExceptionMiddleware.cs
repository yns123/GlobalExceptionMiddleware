using System.Net;
using System.Text.Json;

namespace MiddlewareSample.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor'da ILogger'ı inject et
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Sonraki middleware'e geç
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata yakalandığında özel bir işlem yap
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            // Loglama işlemi (loglama kütüphanenize göre özelleştirilebilir)
            Console.WriteLine($"Exception: {exception.Message}");

            // HTTP yanıtını ayarla
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred.",
                Detail = exception.Message // Detayları istemcide göstermemek genellikle daha güvenlidir
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
