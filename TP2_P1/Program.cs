using Microsoft.EntityFrameworkCore;
using TP2_P1.Models.EntityFramework;
using TP2_P1.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSeriesFrontend",
        policy =>
        {
            policy
                .WithOrigins("https://apiservicev8-btc8ajawd9bme4gm.switzerlandnorth-01.azurewebsites.net")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddRazorPages();

builder.Services.AddDbContext<SeriesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SeriesDbContextRemote")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();
