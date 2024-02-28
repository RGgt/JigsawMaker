using Carter;
using JigsawMakerApi.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace JigsawMakerApi.Features.BackgroundImages;

public class ListBackgroundImagesEndoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/background-images", async (HttpResponse res, AppDbContext dbContext) => {
            var backgroundImages = await dbContext.BackgroundImages.ToListAsync(CancellationToken.None);
            return Results.Ok(backgroundImages);
        })
        .WithOpenApi()
        .WithSummary("Get Background Images")
        .WithDescription("Retrieves a list of background images that are ready to be used as source for jigsaw puzzles");
    }
}


