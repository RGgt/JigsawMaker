using Carter;
using JigsawMakerApi.Configuration;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.DataAccess;
using Microsoft.Extensions.Options;

namespace JigsawMakerApi.Features.BackgroundImages;

public class ViewBackgroundImage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/static/{imageName}", async (string imageName, HttpResponse res, AppDbContext dbContext, IBlobStorageService blobStorageService, IOptions < AzureBlobStorageOptions> options, IKeyVaultService keyVaultService,
        CancellationToken cancellationToken) =>
        {
            using (var memoryStream = await blobStorageService.ReadStaticFile(imageName, cancellationToken))
            {
                if (memoryStream is null) 
                    return Results.NotFound();
                return Results.File(memoryStream.ToArray(), "image/png");
            }
        })
        .WithOpenApi()
        .WithSummary("Serve a static image")
        .WithDescription("Retrieves a an image from a special folder inside the Azure Storage Blob and serves it forward.");
    }
}
