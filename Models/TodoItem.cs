namespace TodoApi.Models
{
    /// <summary>
    /// Представляет задачу в списке дел (TODO).
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Уникальный идентификатор задачи.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название задачи.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата создания задачи.
        /// Устанавливается автоматически при создании.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Статус завершения задачи.
        /// True, если задача выполнена; False, если нет.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}