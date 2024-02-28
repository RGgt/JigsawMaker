using Carter;
using FluentValidation;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Birds;

public class ListBirds
{
    public class Query : IRequest<Result<List<Bird>>>
    {
        public int Skip { get; set; } = 0;
        public int Count { get; set; } = 10;
        public string Specie { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    };
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(c => c.Skip).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Count).GreaterThanOrEqualTo(5);
        }
    }
    internal sealed class Handler : IRequestHandler<Query, Result<List<Bird>>>
    {
        private readonly IValidator<Query> _validator;
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext, IValidator<Query> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<List<Bird>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure<List<Bird>>(new Error("ListBirds.Validation", validRequest.ToString()));
            }
            var birds = (from bird in _dbContext.Birds
                         where (bird.Location == request.Location || request.Location == string.Empty) && (bird.Specie == request.Specie || request.Specie == string.Empty)
                         select bird)
                        .Skip(request.Skip)
                        .Take(request.Count);
            return Result.Success(await birds.ToListAsync(cancellationToken));
        }
    }
}

public class ListBirdsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/birds/list", async (ISender sender, [FromQuery]int skip=0, [FromQuery] int size=20, [FromQuery] string specie="", [FromQuery] string location="" ) =>
        {
            ListBirds.Query query = new (){
                Location=location,
                Skip=skip,
                Specie=specie,
                Count=size
            };
            var response = await sender.Send(query);
            if (response.IsFailure) return Results.StatusCode(StatusCodes.Status500InternalServerError);
            return Results.Ok(response.Value);
        })
        .WithOpenApi()
        .WithSummary("List birds")
        .WithDescription("Retrieves a paginated list of birds photos, optionally filtered by specie or photo location");
    }
}