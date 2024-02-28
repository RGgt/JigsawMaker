using Carter;
using JigsawMakerApi.Authorization;
using JigsawMakerApi.Contracts;
using MediatR;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Users;

public class GenerateToken 
{
    public class Query : IRequest<Result<string>>
    {
        public int UserId { get; set; } = 0;
        public int BirdId { get; set; }
        public IFormFile File { get; set; }
    }
}
public class GenerateTokenEndoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/generate", async (AuthenticateRequest request, IAuthenticationTokenHelper authenticationTokenHelper, IUserService userService) =>
        {
            var response = userService.Authenticate(request);

            if (response == null)
                return Results.Unauthorized();

            return Results.Ok(response);
        })
        .WithOpenApi()
        .WithSummary("Generate JTW")
        .WithDescription("Receives user credentials, authenticate it and generates a JWT toke which is returned in the response.")
        .AllowAnonymous();
    }
}