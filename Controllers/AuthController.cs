using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    /// <summary>
    /// Контроллер для управления аутентификацией пользователей.
    /// Обрабатывает запросы на вход и генерацию JWT-токенов.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _tokenService;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="AuthController"/>.
        /// </summary>
        /// <param name="tokenService">Сервис для генерации JWT-токенов.</param>
        public AuthController(JwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Выполняет проверку данных пользователя и возвращает JWT-токен при успешной аутентификации.
        /// </summary>
        /// <param name="model">Модель входа, содержащая имя пользователя и пароль.</param>
        /// <returns>
        /// Статус 200 с токеном в случае успешной аутентификации.
        /// Статус 401, если имя пользователя или пароль неверны.
        /// </returns>
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Проверка пользователя.
            // Здесь "admin" и "password" используются в рамках демонстрации, в реальном проекте использование базы данных или внешних сервисов.
            if (model.Username == "admin" && model.Password == "password")
            {
                // Генерация токена для пользователя
                var token = _tokenService.GenerateToken(model.Username);

                // Возвращаем токен клиенту
                return Ok(new { Token = token });
            }

            // Возвращаем статус "Unauthorized", если данные не совпадают
            return Unauthorized();
        }
    }
}