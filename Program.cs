using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ����������� SqlServer
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� ��������� ������������ � NewtonsoftJson
builder.Services.AddControllers().AddNewtonsoftJson();

// ��������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // ��������� ��������� XML-������������, ���� ��������
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ��������� ����
builder.Services.AddLogging();

// ��������� � ��������� ����������
var app = builder.Build();

// ������������� ���� ������
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ������������� ���� ������: {ex.Message}");
        throw;
    }
}

// Swagger ������ � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// ��������� ���������
app.MapControllers();

// ������ ����������
app.Run();