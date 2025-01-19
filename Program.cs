using System.Text;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using TodoApi.Data;
using TodoApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
 
var builder = WebApplication.CreateBuilder(args);

// Подключение к SQL Server
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка JWT
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtConfig["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Регистрация JwtTokenService
builder.Services.AddSingleton<JwtTokenService>();

// Добавляем поддержку контроллеров с NewtonsoftJson
builder.Services.AddControllers().AddNewtonsoftJson();

// Настройка API-версионирования
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Добавляем Swagger
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Todo API v1",
        Version = "v1",
        Description = "API для работы с задачами (версия 1.0)"
    });

    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Todo API v2",
        Version = "v2",
        Description = "API для работы с задачами (версия 2.0)"
    });

    c.TagActionsBy(api => new[] { api.GroupName ?? "default" });
});

// Генерация и настройка приложения
var app = builder.Build();

// Включение аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Инициализация базы данных
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка инициализации базы данных: {ex.Message}");
        throw;
    }
}

// Настройка Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Todo API V2");
});

// Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Настройка маршрутов
app.MapControllers();

// Запуск приложения
app.Run();