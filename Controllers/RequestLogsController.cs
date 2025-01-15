using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с логами запросов к API.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequestLogsController : ControllerBase
    {
        #region Fields

        private readonly TodoContext _context;

        #endregion

        #region Context

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RequestLogsController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных для доступа к логам запросов.</param>
        public RequestLogsController(TodoContext context)
        {
            _context = context;
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Возвращает список всех логов запросов.
        /// </summary>
        /// <returns>Список объектов <see cref="RequestLog"/>.</returns>
        // GET: api/requestlogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetRequestLogs()
        {
            return await _context.RequestLogs.ToListAsync();
        }

        /// <summary>
        /// Возвращает лог запроса по указанному идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор лога запроса.</param>
        /// <returns>Объект <see cref="RequestLog"/>, если найден; иначе статус 404 (NotFound).</returns>
        // GET: api/requestlogs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLog>> GetRequestLog(int id)
        {
            var requestLog = await _context.RequestLogs.FindAsync(id);

            if (requestLog == null)
            {
                return NotFound();
            }

            return requestLog;
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Возвращает логи запросов постранично.
        /// </summary>
        /// <param name="page">Номер страницы (начиная с 1).</param>
        /// <param name="pageSize">Количество записей на одной странице.</param>
        /// <returns>Объект с метаданными пагинации и списком логов.</returns>
        /// <remarks>
        /// Возвращает объект со следующими полями:
        /// - `TotalItems` — общее количество логов.
        /// - `TotalPages` — общее количество страниц.
        /// - `CurrentPage` — текущая страница.
        /// - `Logs` — список логов на текущей странице.
        /// </remarks>
        [HttpGet("paged")]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetPagedRequestLogs(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Page and PageSize must be greater than 0.");
            }

            var totalItems = await _context.RequestLogs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page > totalPages)
            {
                return BadRequest($"Page {page} does not exist. Total pages: {totalPages}");
            }

            var logs = await _context.RequestLogs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                Logs = logs
            });
        }

        #endregion
    }
}