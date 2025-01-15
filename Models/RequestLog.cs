namespace TodoApi.Models
{
    /// <summary>
    /// Представляет лог запроса к API.
    /// </summary>
    public class RequestLog
    {
        /// <summary>
        /// Уникальный идентификатор лога запроса.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// IP-адрес клиента, с которого был выполнен запрос.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// HTTP-метод запроса (например, GET, POST, PUT, DELETE).
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Путь запроса (URL), указанный клиентом.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Время выполнения запроса в формате UTC.
        /// Устанавливается автоматически при создании объекта.
        /// </summary>
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }
}