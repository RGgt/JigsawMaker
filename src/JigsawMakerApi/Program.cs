//#define RUN_MIGRAITONS
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Carter;
using JigsawMakerApi.Authorization;
using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;

namespace JigsawMakerApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Read configurations
        // READ SECRET FROM AZURE KEYS VAULT

        // Configures the application using settings from Azure Key Vault and app configuration:

        // 1.Retrieve Azure Vault name from configuration and validate its presence
        var azureVaultName = builder.Configuration["AzureVaultName"];
        if(azureVaultName is null) 
            throw new ArgumentNullException(nameof(azureVaultName));

        // 2. Connect to Azure Key Vault with retries configured
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

        // 3. Retrieve database password from Azure Key Vault
        KeyVaultSecret secret = client.GetSecret("DbPassword");
        string dbPassword = secret.Value;

        // 4. Compose complete database connection string
        var partialConnectionString = builder.Configuration.GetConnectionString("Database");
        var conStrBuilder = new SqlConnectionStringBuilder(partialConnectionString)
        {
            Password = dbPassword
        };
        var connectionString = conStrBuilder.ConnectionString;

        // 5. Configure services using configuration sections
        builder.Services.Configure<AzureBlobStorageOptions>(options =>
        {
            builder.Configuration.GetSection("AzureBlobStorage").Bind(options);
        });
        builder.Services.Configure<AzureKeyVaultOptions>(options =>
        {
            builder.Configuration.GetSection("AzureKeyVault").Bind(options);
        });
        builder.Services.Configure<AppSettings>(options =>
        {
            builder.Configuration.GetSection("AppSettings").Bind(options);
        });

        // 6. Validate app settings and retrieve JWT secret
        var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
        if (appSettings == null || appSettings.Secret == null)
            throw new NotSupportedException("This service cannot run without having a `Secret` setting configured");
        var jwtSecret = appSettings.Secret;
        #endregion

        #region Configure DI
        // Registers services with the dependency injection container:
        builder.Services.AddScoped<IFileNameService, FileNameService>();
        builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
        builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
        builder.Services.AddScoped<IConnectionStringBuilderService, ConnectionStringBuilderService>();
        builder.Services.AddSingleton<IAuthenticationTokenHelper>(provider => new AuthenticationTokenHelper(jwtSecret));
        builder.Services.AddScoped<IJwtUtils, JwtUtils>();
        builder.Services.AddScoped<IUserService, UserService>();
        #endregion

        // Configures the Antiforgery service to use the header named "X-XSRF-TOKEN" for CSRF protection.
        builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

        // Configures the dependency injection container to provide an instance of the `AppDbContext` class, 
        // using the specified connection string to connect to a SQL Server database.
        builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

        // Add Carter for automatic maping of enpoints. 
        // Endpoints definition is in Fratures/[Folder]/[File]/[File]Endpoint
        // Also add MediatR and Fluent Validations
        builder.Services.AddCarter();
        var assembly=typeof(Program).Assembly;
        // Registers all MediatR handlers  found in the current assembly.
        builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(assembly));
        // Registers all public validator classes found in the current assembly.
        builder.Services.AddValidatorsFromAssembly(assembly);



        // Configures authentication for the application using JWT Bearer tokens:
        builder.Services.AddAuthentication(options =>
        {
            // Set JWT Bearer as the default authentication and challenge schemes.
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            // Retrieve the configured token validation parameters from the IAuthenticationTokenHelper service.
            var authenticationTokenHelper = builder.Services.BuildServiceProvider().GetRequiredService<IAuthenticationTokenHelper>();
            var validationParameters = authenticationTokenHelper.GetTokenValidationParameters();
            options.TokenValidationParameters = validationParameters;
        });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Configures Swagger for API documentation:
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            // Configures Swagger to include authorization details:
            options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization"
            });
        });

        // Configures CORS (Cross-Origin Resource Sharing) to allow requests from any origin using the "AllowAll" policy.
        // This approach is permissive and should be used with caution, especially in production environments.
        // Consider restricting allowed origins, methods, and headers for better security.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });



        var app = builder.Build();

        app.UseAntiforgery();
#if RUN_MIGRAITONS
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
#endif

        // Configures the ASP.NET Core middleware pipeline:
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        // Allow all origins for CORS requests (**caution, be careful with this in production!**)
        app.UseCors("AllowAll");
        // Enable Swagger for documentation, regardless of environment.
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapCarter();

        // Configures an endpoint to generate and return an Antiforgery request token for CSRF protection.
        app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
        {
            var tokens = forgeryService.GetAndStoreTokens(context);
            var xsrfToken = tokens.RequestToken!;
            return TypedResults.Content(xsrfToken, "text/plain");
        });

        app.Run();

    }
}
