using BankProcessor.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
{
// Add services to the container.
//builder.Services.AddDbContext<>
builder.Services.AddControllers();
builder.Services.AddDbContext<StatementContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}

