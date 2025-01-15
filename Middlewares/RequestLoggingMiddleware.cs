using TodoApi.Data;
using TodoApi.Models;

/// <summary>
/// Middleware для логирования всех входящих запросов к API.
/// </summary>
public class RequestLoggingMiddleware
{
    #region Fields

    /// <summary>
    /// Делегат, представляющий следующий компонент в пайплайне обработки запросов.
    /// </summary>
    private readonly RequestDelegate _next;

    #endregion

    #region Context

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RequestLoggingMiddleware"/>.
    /// </summary>
    /// <param name="next">Делегат для передачи запроса следующему компоненту.</param>
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    #endregion

    #region Public

    /// <summary>
    /// Обрабатывает входящий запрос и логирует его информацию в базу данных.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    /// <param name="dbContext">Контекст базы данных, используемый для сохранения логов.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task InvokeAsync(HttpContext context, TodoContext dbContext)
    {
        // Проверяем, не обращается ли запрос к эндпоинту логов
        if (context.Request.Path.StartsWithSegments("/api/requestlogs", StringComparison.OrdinalIgnoreCase))
        {
            // Передаём запрос дальше без логирования
            await _next(context);
            return;
        }

        // Логируем информацию о запросе
        var log = new RequestLog
        {
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            HttpMethod = context.Request.Method,
            Path = context.Request.Path,
            RequestTime = DateTime.UtcNow
        };

        // Сохраняем лог в базу данных
        dbContext.RequestLogs.Add(log);
        await dbContext.SaveChangesAsync();

        // Передаём запрос следующему компоненту в пайплайне
        await _next(context);
    }

    #endregion
}