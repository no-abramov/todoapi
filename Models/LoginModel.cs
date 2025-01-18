namespace TodoApi.Models
{
    /// <summary>
    /// Модель для передачи данных входа пользователя.
    /// Используется для аутентификации в API.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Имя пользователя.
        /// Обязательное поле.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// Обязательное поле.
        /// </summary>
        public required string Password { get; set; }
    }
}