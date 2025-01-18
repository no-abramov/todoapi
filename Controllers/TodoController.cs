using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с задачами (Todo).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// Контекст базы данных для работы с задачами.
        /// </summary>
        private readonly TodoContext _context;

        #endregion

        #region Context

        /// <summary>
        /// Инициализирует новый экземпляр контроллера <see cref="TodoController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных для задач.</param>
        public TodoController(TodoContext context)
        {
            _context = context;
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Получает список всех задач.
        /// </summary>
        /// <returns>Список задач.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        /// <summary>
        /// Получает задачу по её идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>Задача с указанным идентификатором.</returns>
        /// <remarks>
        /// Требует авторизации JWT.
        /// </remarks>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        /// <summary>
        /// Создаёт новую задачу.
        /// </summary>
        /// <param name="todoItem">Объект задачи для создания.</param>
        /// <returns>Созданная задача с её идентификатором.</returns>
        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            todoItem.CreatedDate = DateTime.UtcNow; // Устанавливаем дату создания
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        /// <summary>
        /// Обновляет существующую задачу.
        /// </summary>
        /// <param name="id">Идентификатор задачи для обновления.</param>
        /// <param name="todoItem">Обновлённые данные задачи.</param>
        /// <returns>Результат обновления (NoContent, BadRequest или NotFound).</returns>
        /// <remarks>
        /// Требует авторизации JWT.
        /// </remarks>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(int id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Удаляет задачу по её идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор задачи для удаления.</param>
        /// <returns>Результат удаления (NoContent или NotFound).</returns>
        /// <remarks>
        /// Требует авторизации JWT.
        /// </remarks>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Patch

        /// <summary>
        /// Частично обновляет данные задачи.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <param name="patchDoc">Документ для частичного обновления задачи.</param>
        /// <returns>Результат обновления (NoContent, BadRequest или NotFound).</returns>
        /// <remarks>
        /// Требует авторизации JWT.
        /// </remarks>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTodoItem(int id, [FromBody] JsonPatchDocument<TodoItem> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(todoItem, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Получает список задач постранично.
        /// </summary>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Количество записей на одной странице.</param>
        /// <returns>Объект с метаданными пагинации и списком задач.</returns>
        [HttpGet("paged")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetPagedTodoItems(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and PageSize must be greater than 0.");
            }

            var totalItems = await _context.TodoItems.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page > totalPages)
            {
                return BadRequest($"Page {page} does not exist. Total pages: {totalPages}");
            }

            var todoItems = await _context.TodoItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                Items = todoItems
            });
        }

        #endregion

        #region Private

        /// <summary>
        /// Проверяет, существует ли задача с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>True, если задача существует; иначе False.</returns>
        private bool TodoItemExists(int id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        #endregion
    }
}