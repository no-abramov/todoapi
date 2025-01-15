using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{
    /// <summary>
    /// Контекст базы данных для приложения TodoApi.
    /// </summary>
    public class TodoContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TodoContext"/> с указанными параметрами.
        /// </summary>
        /// <param name="options">Параметры конфигурации контекста базы данных.</param>
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        /// <summary>
        /// Представляет таблицу задач (TodoItems) в базе данных.
        /// </summary>
        public DbSet<TodoItem> TodoItems { get; set; }

        /// <summary>
        /// Представляет таблицу логов запросов (RequestLogs) в базе данных.
        /// </summary>
        public DbSet<RequestLog> RequestLogs { get; set; }
    }
}