using Carter;
using FluentValidation;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Birds;

public class AddBird
{
    public class Command : IRequest<Result<int>>
    {
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
    internal sealed class Handler : IRequestHandler<Command, Result<int>>
    {
        private readonly IValidator<Command> _validator;
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext, IValidator<Command> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure<int>(new Error("AddBird.Validation", validRequest.ToString()));
            }
            Bird b = new()
            {
                Date = request.Date,
                ImageUrl = request.ImageUrl,
                Location = request.Location,
                Specie = request.Specie,
                UserId = request.UserId
            };
            _dbContext.Birds.Add(b);
            await _dbContext.SaveChangesAsync();
            return Result.Success(b.Id);
        }
    }
}

public class AddBirdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/birds", async (ISender sender,[FromForm] AddBird.Command command) =>
        {
            //TODO: get user id (from token or context) and set it to command.UserId
            command.UserId = 1;
            command.ImageUrl = "MISSING_FILE.png";
            var response = await sender.Send(command);
            if (response.IsFailure) return Results.BadRequest();
            return Results.Created($"/api/birds/{response.Value}", response.Value);
        })
        .WithOpenApi()
        .WithSummary("Adds a bird to the database")
        .WithDescription("Adds a bird to the database and returns its Id")
        .DisableAntiforgery();
    }
}