
using Carter;
using FluentValidation;
using JigsawMakerApi.Contracts;
using JigsawMakerApi.DataAccess;
using JigsawMakerApi.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PuzzleMakerApi.Domain.Shared;

namespace JigsawMakerApi.Features.UserFiles;

public class UploadUserFile
{
    public class Command : IRequest<Result<string>>
    {
        public int UserId { get; set; } = 0;
        public int BirdId { get; set; }
        public IFormFile File { get; set; }
    }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId).GreaterThan(0);
            RuleFor(c => c.BirdId).GreaterThan(0);
            RuleFor(c => c.File).NotNull();
            RuleFor(c => c.File.Length).GreaterThan(0);
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IValidator<Command> _validator;
        private readonly AppDbContext _dbContext;
        private readonly IBlobStorageService _storageService;
        private readonly IFileNameService _fileNameService;

        public Handler(AppDbContext dbContext, IValidator<Command> validator, IBlobStorageService storageService, IFileNameService fileNameService)
        {
            _dbContext = dbContext;
            _validator = validator;
            _storageService = storageService;
            _fileNameService = fileNameService;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // 1. Validate the incoming request
            var validRequest = _validator.Validate(request);
            if (!validRequest.IsValid)
            {
                return Result.Failure<string>(new Error("UploadUserFile.Validation", validRequest.ToString()));
            }
            // 2. Find the bird record based on ID and user ID
            var bird = await _dbContext.Birds.FindAsync(request.BirdId);
            if (bird == null) return Result.Failure<string>(new Error("UploadUserFile.Validation", "Record not found"));
            if (bird.UserId != request.UserId) return Result.Failure<string>(new Error("UploadUserFile.Validation", "Record not found"));
            // 3. Generate a new file name
            string fileName = _fileNameService.GenertateNew(request.File.FileName);
            // 4. Wrap operations in a database transaction
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try {
                    // 5. Track the old file name and record (if exists)
                    string oldFileName = bird.ImageUrl;
                    // 6. Create a new user file record
                    var oldFile = _dbContext.UserFiles.FirstOrDefault(f => f.StoreName == bird.ImageUrl);
                    UserFile newFile = new()
                    {
                        IsUploaded=false,
                        UploadStartDate=DateTime.UtcNow,
                        StoreName= fileName,
                        DeleteDate=null,
                        IsDeleted=false,
                        OriginalName=request.File.FileName,

                    };
                    // 7. Persist the new file record
                    _dbContext.Add(newFile);
                    await _dbContext.SaveChangesAsync();
                    // 8. Save the uploaded file using the storage service
                    await _storageService.WriteStaticFile(request.File, fileName, cancellationToken);
                    // 9. Mark the new file as uploaded 
                    newFile.IsUploaded = true;
                    // 10. Update the bird record with the new file name
                    bird.ImageUrl = fileName;
                    // 11. Handle potential old file deletion (if it exists)
                    await _dbContext.SaveChangesAsync();
                    if (oldFile is not null)
                    {
                        // Delete the old file from storage (OPTIONAL)
                        await _storageService.DeleteStaticFile(oldFileName, cancellationToken);
                        // Mark the old file as deleted in the database
                        oldFile.IsDeleted = true;
                        oldFile.DeleteDate = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                    // 12. Commit the transaction on success
                    transaction.Commit();
                }
                catch {
                    transaction.Rollback();
                    return Result.Failure<string>(new Error("UploadUserFile.Upload", "Multi-step operation failed"));
                }
            }
            // 13. Return the name of the uploaded file on success
            return Result.Success(fileName);
        }
    }
}

public class UpdateBirdEndpoint : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/uploads/{birdId}/", async ([FromRoute] int birdId, IFormFile file, ISender sender) =>
        {
            UploadUserFile.Command c = new() {
                BirdId = birdId,
                File= file,
                //TODO: get user id(from token or context) and set it to command.UserId
                UserId = 1
            };
            var response = await sender.Send(c);
            if (response.IsFailure) return Results.BadRequest();
            return Results.Ok(response.Value);
        })
        .WithSummary("Upload image")
        .WithDescription("Uploads an image associated with a bird")
        .DisableAntiforgery();
    }
}