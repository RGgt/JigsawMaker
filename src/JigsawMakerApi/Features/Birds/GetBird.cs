using Carter;
using FluentValidation;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Entities;
using MediatR;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.Birds;

public class GetBird
{
    public class Query : IRequest<Result<Bird>>
    {
        public int Id { get; set; } = 0;
    };
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
        }
    }
    internal sealed class Handler : IRequestHandler<Query, Result<Bird>>
    {
        private readonly IValidator<Query> _validator;
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext, IValidator<Query> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<Bird>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure<Bird>(new Error("GetBird.Validation", validRequest.ToString()));
            }
            var bird = _dbContext.Birds.FirstOrDefault(b => b.Id == request.Id);
            if(bird is null) return Result.Failure<Bird>(new Error("GetBird.Validation", "Record not found"));
            return Result.Success(bird);
        }
    }
}

public class GetBirdEndpoint : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/birds/{id}",  async (ISender sender,int id) =>
        {
            GetBird.Query query = new (){
                Id=id
            };
            var response = await sender.Send(query);
            if (response.IsFailure) return Results.NotFound();
            return Results.Ok(response.Value);
        })
        .WithOpenApi()
        .WithSummary("Gets bird details by id")
        .WithDescription("Gets bird details by id");
    }
}