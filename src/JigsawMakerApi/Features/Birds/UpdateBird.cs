using Carter;
using FluentValidation;
using JigsawMakerApi.DataAccess;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Birds;

public class UpdateBird
{
    public class Command : IRequest<Result>
    {
        public int Id { get; set; } = 0;
        public string ImageUrl { get; set; }
        public string Location { get; set; }
        public string Specie { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
    };
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Location).NotNull().NotEmpty();
            RuleFor(c => c.Specie).NotNull().NotEmpty();
            RuleFor(c => c.Date).LessThan(DateTime.UtcNow);
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IValidator<Command> _validator;
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure(new Error("UpdateBird.Validation", validRequest.ToString()));
            }
            var bird = await _dbContext.Birds.FindAsync(request.Id);
            if (bird == null) return Result.Failure(new Error("UpdateBird.Validation", "Record not found"));
            if (bird.UserId != request.UserId) return Result.Failure(new Error("UpdateBird.Validation", "Record not found"));
            bird.ImageUrl = request.ImageUrl;
            bird.Location = request.Location;
            bird.Specie = request.Specie;
            bird.Date = request.Date;
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }
    }
}

public class UpdateBirdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/birds/{id}", async (ISender sender, [FromForm] AddBird.Command command, int id) =>
        {
            UpdateBird.Command c = new()
            {
                Id = id,
                Date = command.Date,
                ImageUrl = command.ImageUrl,
                Location = command.Location,
                Specie = command.Specie,
                UserId = command.UserId,
            };
            //TODO: get user id(from token or context) and set it to command.UserId
            command.UserId = 1;
            var response = await sender.Send(c);
            if (response.IsFailure) return Results.BadRequest();
            return Results.NoContent();
        })
        .WithOpenApi()
        .WithSummary("Updates a bird to the database")
        .WithDescription("Updates a bird to the database")
        .DisableAntiforgery();

    }
}