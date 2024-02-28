using Carter;
using FluentValidation;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.DataAccess;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Birds;

public class DeleteBird
{
    public class Command : IRequest<Result>
    {
        public int Id { get; set; } = 0;
        public int UserId { get; set; }
    };
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IValidator<Command> _validator;
        private readonly AppDbContext _dbContext;
        private readonly IBlobStorageService _storageService;

        public Handler(AppDbContext dbContext, IValidator<Command> validator, IBlobStorageService storageService)
        {
            _dbContext = dbContext;
            _validator = validator;
            _storageService = storageService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure(new Error("DeleteBird.Validation", validRequest.ToString()));
            }
            var bird = _dbContext.Birds.FirstOrDefault(b => b.Id == request.Id);
            if (bird == null) return Result.Failure(new Error("DeleteBird.Validation", "Record not found"));
            if (bird.UserId != request.UserId) return Result.Failure(new Error("UpdateBird.Validation", "Record not found"));
            await _storageService.DeleteStaticFile(bird.ImageUrl, cancellationToken);
            _dbContext.Birds.Remove(bird);
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }
    }
}

public class DeleteBirdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/birds/{id}", async (ISender sender, [FromRoute] int id) =>
        {
            DeleteBird.Command query = new()
            {
                Id = id,
                //TODO: get user id(from token or context) and set it to command.UserId
                UserId = 1
            };
            var response = await sender.Send(query);
            if (response.IsFailure) return Results.NotFound();
            return Results.NoContent();
        })
        .WithOpenApi()
        .WithSummary("Deletes a bird by id")
        .WithDescription("Deletes a bird by id")
        .DisableAntiforgery();
    }
}