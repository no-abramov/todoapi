using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Подключение SqlServer
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем поддержку контроллеров с NewtonsoftJson
builder.Services.AddControllers().AddNewtonsoftJson();

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Добавляем поддержку XML-комментариев, если включены
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Добавляем логи
builder.Services.AddLogging();

// Генерация и настройка приложения
var app = builder.Build();

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

// Swagger только в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Настройка маршрутов
app.MapControllers();

// Запуск приложения
app.Run();