
using Carter;
using JigsawMakerApi.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JigsawMakerApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // read partial connection string from configuration
        var partialConnectionString = builder.Configuration.GetConnectionString("Database");
        // compose complet connection string loading password from secrets
        var conStrBuilder = new SqlConnectionStringBuilder(partialConnectionString)
        {
            Password = builder.Configuration["DbPassword"]
        };
        var connectionString = conStrBuilder.ConnectionString;

        // Add DbContext
        builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

        // Add Carter for automatic maping of enpoints. 
        // Endpoints definition is in Fratures/[Folder]/[File]/[File]Endpoint
        builder.Services.AddCarter();

        // Add services to the container.
        builder.Services.AddAuthorization();

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

        // Map all endpoints defined with Carter
        app.MapCarter();

        app.Run();
    }
}
