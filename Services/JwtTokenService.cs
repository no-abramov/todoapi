using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace TodoApi.Services
{
    /// <summary>
    /// Сервис для работы с JWT-токенами.
    /// Отвечает за генерацию токенов, используемых для аутентификации.
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="JwtTokenService"/>.
        /// </summary>
        /// <param name="configuration">Конфигурация приложения для получения параметров JWT.</param>
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Генерирует JWT-токен для указанного имени пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя, для которого создаётся токен.</param>
        /// <returns>Сгенерированный JWT-токен в виде строки.</returns>
        public string GenerateToken(string username)
        {
            // Получение настроек JWT из конфигурации
            var jwtConfig = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtConfig["Key"]);

            // Создание обработчика токенов
            var tokenHandler = new JwtSecurityTokenHandler();

            // Настройка токена
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Установка данных для токена (например, Claims)
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                // Установка срока действия токена
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["TokenLifetime"])),
                // Указание издателя и аудитории
                Issuer = jwtConfig["Issuer"],
                Audience = jwtConfig["Audience"],
                // Установка ключа подписи токена
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Генерация токена
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}