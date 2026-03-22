using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Middleware;
using TodoApp.Core;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TodoApp API",
        Version = "v1",
        Description = "A clean architecture To-Do List API POC",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Loic Ndjoyi",
            Url = new Uri("https://github.com/loicndjoyi")
        }
    });

    // Automatically include XML Comments if the file exists
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure Global Exception Handling (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure SQLite Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todoapp.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Dependency Injection
builder.Services.AddScoped<ITodoService, TodoService>();

// Configure CORS for Angular Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // <-- Added global error handling middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularClient");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
