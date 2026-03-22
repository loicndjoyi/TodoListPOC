using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Middleware;
using TodoApp.Core;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseCors("AllowAngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
