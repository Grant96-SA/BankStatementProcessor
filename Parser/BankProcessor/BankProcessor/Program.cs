using BankProcessor.Models;
using BankProcessor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;


var builder = WebApplication.CreateBuilder(args);

// log filter
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<StatementContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the StatementProcessor service
builder.Services.AddScoped<StatementParser>();
builder.Services.AddSingleton<StatementProcessingService>();
builder.Services.AddHostedService<StatementProcessingService>(provider => provider.GetService<StatementProcessingService>());
//builder.Services.AddHostedService<StatementProcessingService>();

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}

