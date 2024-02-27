
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Carter;
using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JigsawMakerApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // READ SECRET FROM AZURE KEYS VAULT

        // Get key vault name/address
        var azureVaultName = builder.Configuration["AzureVaultName"];
        if(azureVaultName is null) 
            throw new ArgumentNullException(nameof(azureVaultName));

        // connect to keys vault
        SecretClientOptions options = new()
        {
            Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
        };
        var client = new SecretClient(new Uri(azureVaultName), new DefaultAzureCredential(), options);

        // get database password from keys vault
        KeyVaultSecret secret = client.GetSecret("DbPassword");
        string dbPassword = secret.Value;
        // GET DATABASE CONNECITON STRING

        // read partial connection string from configuration
        var partialConnectionString = builder.Configuration.GetConnectionString("Database");

        // compose complet connection string loading password retrieved from the keys vault
        var conStrBuilder = new SqlConnectionStringBuilder(partialConnectionString)
        {
            Password = dbPassword
        };
        var connectionString = conStrBuilder.ConnectionString;

        //
        builder.Services.Configure<AzureBlobStorageOptions>(options =>
        {
            builder.Configuration.GetSection("AzureBlobStorage").Bind(options);
        });

        builder.Services.Configure<AzureKeyVaultOptions>(options =>
        {
            builder.Configuration.GetSection("AzureKeyVault").Bind(options);
        });


        builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<IConnectionStringBuilderService, ConnectionStringBuilderService>();

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
