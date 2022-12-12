using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nest;
using ReadModel.Elastic;
using ReadModel.Models;
using ReadModel.Pg;
using ReadModel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PgContext>();
builder.Services.AddScoped<ElasticRepository>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddSingleton<ElasticClient>(s => new ElasticClient(new Uri("http://localhost:9200/")));

var app = builder.Build();
// специальная настройка для работы с датой в ПГ
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.MapGet("api/employees", async ([FromServices] EmployeeService service) => await service.All());

app.MapGet("api/employees/{id}", async (int id, [FromServices] EmployeeService service) => await service.ById(id));

app.MapPost("api/employees", async ([FromBody] Employee employee, [FromServices] EmployeeService service) => await service.Create(employee));

app.MapPut("api/employees", async ([FromBody] Employee employee, [FromServices] EmployeeService service) => await service.Update(employee));

app.MapDelete("api/employees/{id}", async (int id, [FromServices] EmployeeService service) => await service.Delete(id));

app.MapGet("api/employees/search", async (
    [FromServices] EmployeeService service,
    [FromServices] ILogger<EmployeeService> logger,
    string text, 
    string? city,
    string? university,
    DateTime? fromStartDate) => 
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await service.Search(text, city, university, fromStartDate);
        logger.LogInformation("search {time} ms", stopwatch.ElapsedMilliseconds);
        stopwatch.Stop();
        return result;
    });

app.Run();