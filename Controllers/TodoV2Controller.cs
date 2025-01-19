using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с задачами (Todo v2).
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(2)]
    [ApiExplorerSettings(GroupName = "v2")]
    public class TodoV2Controller : ControllerBase
    {
        private readonly TodoContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера v2 <see cref="TodoV2Controller"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных для задач.</param>
        public TodoV2Controller(TodoContext context)
        {
            _context = context;
        }

        // GET: api/v2.0/todo
        /// <summary>
        /// Получить список всех задач, но с фильтром по состоянию.
        /// </summary>
        /// <param name="isCompleted">Фильтр: только выполненные или невыполненные задачи.</param>
        /// <returns>Список задач, соответствующих фильтру.</returns>
        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetFilteredTodoItems(bool? isCompleted = null)
        {
            var query = _context.TodoItems.AsQueryable();

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            return await query.ToListAsync();
        }

        // PATCH: api/v2.0/todo/{id}/toggle
        /// <summary>
        /// Переключить состояние задачи (выполнено/не выполнено).
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>Обновленная задача.</returns>
        [HttpPatch("{id}/toggle")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> ToggleTodoItemCompletion(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.IsCompleted = !todoItem.IsCompleted;
            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        // DELETE: api/v2.0/todo/cleanup
        /// <summary>
        /// Удалить все выполненные задачи.
        /// </summary>
        /// <returns>Количество удаленных задач.</returns>
        [HttpDelete("cleanup")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> CleanupCompletedTasks()
        {
            var completedTasks = _context.TodoItems.Where(t => t.IsCompleted);
            int count = await completedTasks.CountAsync();

            _context.TodoItems.RemoveRange(completedTasks);
            await _context.SaveChangesAsync();

            return Ok(new { DeletedCount = count });
        }

        // GET: api/v2.0/todo/summary
        /// <summary>
        /// Получить сводную информацию о задачах.
        /// </summary>
        /// <returns>Сводная информация: общее количество, выполненные и невыполненные задачи.</returns>
        [HttpGet("summary")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetTodoSummary()
        {
            var totalTasks = await _context.TodoItems.CountAsync();
            var completedTasks = await _context.TodoItems.CountAsync(t => t.IsCompleted);
            var pendingTasks = totalTasks - completedTasks;

            return Ok(new
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks
            });
        }
    }
}